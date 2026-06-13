using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Notification;

public interface IWeChatBindingService
{
    /// <summary>
    /// 为当前登录用户创建微信扫码绑定二维码。
    /// </summary>
    Task<WeChatBindingQrCodeDto> CreateQrCodeAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询当前登录用户的微信绑定状态和待绑定二维码状态。
    /// </summary>
    Task<WeChatBindingStatusDto> GetStatusAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解除当前登录用户的微信绑定。
    /// </summary>
    Task UnbindAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 处理微信公众号扫码或关注事件，将回调 OpenID 绑定到二维码所属用户。
    /// </summary>
    Task HandleScanAsync(string scene, string openId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 微信绑定二维码响应。
/// </summary>
public sealed record WeChatBindingQrCodeDto(string QrCodeUrl, DateTimeOffset ExpiresAt, bool Bound);

/// <summary>
/// 当前用户微信绑定状态。
/// </summary>
public sealed record WeChatBindingStatusDto(
    bool Bound,
    string? OpenIdMasked,
    bool Pending,
    DateTimeOffset? ExpiresAt,
    string? Message);

public class WeChatBindingService(
    ApplicationDbContext dbContext,
    IMemoryCache memoryCache,
    IWeChatOfficialAccountClient weChatClient,
    IOptions<WeChatOfficialAccountOptions> options,
    ILogger<WeChatBindingService> logger) : IWeChatBindingService
{
    private const string TokenCacheKeyPrefix = "wechat-binding:token:";
    private const string UserCacheKeyPrefix = "wechat-binding:user:";
    private const string ResultCacheKeyPrefix = "wechat-binding:result:";
    private static readonly TimeSpan ResultCacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 创建临时二维码并把二维码场景值暂存在内存中，等待微信事件回调消费。
    /// </summary>
    public async Task<WeChatBindingQrCodeDto> CreateQrCodeAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var opt = options.Value;
        if (string.IsNullOrWhiteSpace(opt.AppId) || string.IsNullOrWhiteSpace(opt.Secret))
        {
            throw new KnownException("微信公众号 AppId 或 Secret 未配置，无法生成绑定二维码");
        }

        var user = await dbContext.Users.AsNoTracking()
                       .Where(u => u.Id == userId)
                       .Select(u => new { u.Id, u.WechatGuid })
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new KnownException("当前用户不存在");

        var token = Guid.NewGuid().ToString("N");
        var expireSeconds = Math.Clamp(opt.BindQrExpireSeconds <= 0 ? 300 : opt.BindQrExpireSeconds, 60, 2592000);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expireSeconds);
        var qrResult = await weChatClient.CreateTemporaryQrCodeAsync(token, expireSeconds, cancellationToken);
        if (!qrResult.IsSuccess || string.IsNullOrWhiteSpace(qrResult.QrCodeUrl))
        {
            logger.LogWarning(
                "生成微信绑定二维码失败，用户ID：{UserId}，错误码：{ErrCode}，错误信息：{ErrMsg}",
                userId,
                qrResult.ErrCode,
                qrResult.ErrMsg);
            throw new KnownException($"生成微信绑定二维码失败：{qrResult.ErrMsg ?? qrResult.ErrCode.ToString()}");
        }

        var ticket = new WeChatBindingTicket(userId, expiresAt);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiresAt
        };
        memoryCache.Set(TokenCacheKey(token), ticket, cacheOptions);
        memoryCache.Set(UserCacheKey(userId), new WeChatBindingPending(token, expiresAt), cacheOptions);
        memoryCache.Remove(ResultCacheKey(userId));

        return new WeChatBindingQrCodeDto(qrResult.QrCodeUrl, expiresAt, !string.IsNullOrWhiteSpace(user.WechatGuid));
    }

    /// <summary>
    /// 返回数据库中的实际绑定状态，同时返回内存中的二维码等待状态。
    /// </summary>
    public async Task<WeChatBindingStatusDto> GetStatusAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking()
                       .Where(u => u.Id == userId)
                       .Select(u => new { u.WechatGuid })
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new KnownException("当前用户不存在");

        memoryCache.TryGetValue(UserCacheKey(userId), out WeChatBindingPending? pending);
        memoryCache.TryGetValue(ResultCacheKey(userId), out WeChatBindingResult? result);
        return new WeChatBindingStatusDto(
            !string.IsNullOrWhiteSpace(user.WechatGuid),
            MaskOpenId(user.WechatGuid),
            pending is not null,
            pending?.ExpiresAt,
            result?.Message);
    }

    /// <summary>
    /// 清空用户 OpenID 并失效用户查询缓存。
    /// </summary>
    public async Task UnbindAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new KnownException("当前用户不存在");
        user.UnbindWechat(userId);
        await dbContext.SaveChangesAsync(cancellationToken);

        memoryCache.Remove(UserQuery.GetUserCacheKey(userId));
        memoryCache.Remove(UserCacheKey(userId));
        memoryCache.Set(ResultCacheKey(userId), new WeChatBindingResult("已解除微信绑定"), ResultCacheDuration);
    }

    /// <summary>
    /// 微信回调入口调用此方法完成绑定；二维码过期、用户不存在或 OpenID 已被占用时只记录状态，不阻断微信回调响应。
    /// </summary>
    public async Task HandleScanAsync(string scene, string openId, CancellationToken cancellationToken = default)
    {
        scene = NormalizeScene(scene);
        if (string.IsNullOrWhiteSpace(scene) || string.IsNullOrWhiteSpace(openId))
        {
            logger.LogWarning("微信扫码绑定回调参数为空，场景值：{Scene}，OpenId：{OpenId}", scene, openId);
            return;
        }

        if (!memoryCache.TryGetValue(TokenCacheKey(scene), out WeChatBindingTicket? ticket))
        {
            logger.LogWarning("微信扫码绑定二维码已过期或不存在，场景值：{Scene}", scene);
            return;
        }
        var bindingTicket = ticket!;

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == bindingTicket.UserId, cancellationToken);
        if (user is null)
        {
            SetResult(bindingTicket.UserId, "绑定失败：用户不存在");
            return;
        }

        var duplicated = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id != bindingTicket.UserId && u.WechatGuid == openId, cancellationToken);
        if (duplicated)
        {
            SetResult(bindingTicket.UserId, "绑定失败：该微信已绑定其他OA账号");
            logger.LogWarning("微信绑定失败，同一个 OpenId 已绑定其他用户，用户ID：{UserId}，OpenId：{OpenId}", bindingTicket.UserId, openId);
            return;
        }

        user.BindWechat(openId, bindingTicket.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);

        memoryCache.Remove(TokenCacheKey(scene));
        memoryCache.Remove(UserCacheKey(bindingTicket.UserId));
        memoryCache.Remove(UserQuery.GetUserCacheKey(bindingTicket.UserId));
        SetResult(bindingTicket.UserId, "微信绑定成功");
        logger.LogInformation("微信绑定成功，用户ID：{UserId}，OpenId：{OpenId}", bindingTicket.UserId, openId);
    }

    /// <summary>
    /// 保存短期绑定结果，供前端轮询展示。
    /// </summary>
    private void SetResult(UserId userId, string message)
    {
        memoryCache.Set(ResultCacheKey(userId), new WeChatBindingResult(message), ResultCacheDuration);
    }

    /// <summary>
    /// 新关注事件的场景值会带 qrscene_ 前缀，扫码事件则直接返回原始场景值。
    /// </summary>
    private static string NormalizeScene(string scene)
    {
        scene = scene.Trim();
        return scene.StartsWith("qrscene_", StringComparison.OrdinalIgnoreCase)
            ? scene["qrscene_".Length..]
            : scene;
    }

    /// <summary>
    /// 前端只展示脱敏 OpenID，避免完整标识暴露。
    /// </summary>
    private static string? MaskOpenId(string? openId)
    {
        if (string.IsNullOrWhiteSpace(openId))
        {
            return null;
        }

        var value = openId.Trim();
        return value.Length <= 10 ? $"{value[..Math.Min(3, value.Length)]}****" : $"{value[..6]}****{value[^4..]}";
    }

    private static string TokenCacheKey(string token) => $"{TokenCacheKeyPrefix}{token}";

    private static string UserCacheKey(UserId userId) => $"{UserCacheKeyPrefix}{userId.Id}";

    private static string ResultCacheKey(UserId userId) => $"{ResultCacheKeyPrefix}{userId.Id}";

    /// <summary>
    /// 二维码场景值对应的待绑定用户。
    /// </summary>
    private sealed record WeChatBindingTicket(UserId UserId, DateTimeOffset ExpiresAt);

    /// <summary>
    /// 当前用户正在等待扫码的二维码状态。
    /// </summary>
    private sealed record WeChatBindingPending(string Token, DateTimeOffset ExpiresAt);

    /// <summary>
    /// 最近一次绑定处理结果。
    /// </summary>
    private sealed record WeChatBindingResult(string Message);
}
