using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Ncp.Admin.Web.Application.Services.Notification;

public interface IWeChatOfficialAccountClient
{
    /// <summary>
    /// 发送 OA 通知模板消息。
    /// </summary>
    Task<WeChatTemplateSendResult> SendNoticeTemplateAsync(
        string openId,
        string title,
        string senderName,
        DateTimeOffset createdAt,
        string? url,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成微信公众号临时二维码，用于当前登录用户扫码绑定 OpenID。
    /// </summary>
    Task<WeChatQrCodeCreateResult> CreateTemporaryQrCodeAsync(
        string scene,
        int expireSeconds,
        CancellationToken cancellationToken = default);
}

public record WeChatTemplateSendResult(int ErrCode, string? ErrMsg, long MsgId)
{
    public bool IsSuccess => ErrCode == 0;

    public bool IsInvalidAccessToken => ErrCode is 40001 or 41001 or 42001;
}

/// <summary>
/// 微信临时二维码创建结果。
/// </summary>
public record WeChatQrCodeCreateResult(
    int ErrCode,
    string? ErrMsg,
    string? Ticket,
    int ExpireSeconds,
    string? Url)
{
    public bool IsSuccess => ErrCode == 0 && !string.IsNullOrWhiteSpace(Ticket);

    public bool IsInvalidAccessToken => ErrCode is 40001 or 41001 or 42001;

    /// <summary>
    /// 可直接展示给用户扫码的二维码图片地址。
    /// </summary>
    public string? QrCodeUrl => string.IsNullOrWhiteSpace(Ticket)
        ? null
        : $"https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={Uri.EscapeDataString(Ticket)}";
}

/// <summary>
/// 微信公众号模板消息客户端。
/// </summary>
public class WeChatOfficialAccountClient(
    IHttpClientFactory httpClientFactory,
    IWeChatAccessTokenProvider accessTokenProvider,
    IOptions<WeChatOfficialAccountOptions> options,
    ILogger<WeChatOfficialAccountClient> logger) : IWeChatOfficialAccountClient
{
    public const string HttpClientName = "WeChatOfficialAccount";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<WeChatTemplateSendResult> SendNoticeTemplateAsync(
        string openId,
        string title,
        string senderName,
        DateTimeOffset createdAt,
        string? url,
        CancellationToken cancellationToken = default)
    {
        var result = await SendNoticeTemplateCoreAsync(openId, title, senderName, createdAt, url, forceRefreshToken: false, cancellationToken);
        if (result.IsInvalidAccessToken)
        {
            logger.LogInformation("微信公众号 access_token 已过期或无效，正在刷新后重试一次。");
            result = await SendNoticeTemplateCoreAsync(openId, title, senderName, createdAt, url, forceRefreshToken: true, cancellationToken);
        }

        return result;
    }

    public async Task<WeChatQrCodeCreateResult> CreateTemporaryQrCodeAsync(
        string scene,
        int expireSeconds,
        CancellationToken cancellationToken = default)
    {
        var result = await CreateTemporaryQrCodeCoreAsync(scene, expireSeconds, forceRefreshToken: false, cancellationToken);
        if (result.IsInvalidAccessToken)
        {
            logger.LogInformation("微信公众号 access_token 已过期或无效，正在刷新后重试一次。");
            result = await CreateTemporaryQrCodeCoreAsync(scene, expireSeconds, forceRefreshToken: true, cancellationToken);
        }

        return result;
    }

    private async Task<WeChatTemplateSendResult> SendNoticeTemplateCoreAsync(
        string openId,
        string title,
        string senderName,
        DateTimeOffset createdAt,
        string? url,
        bool forceRefreshToken,
        CancellationToken cancellationToken)
    {
        var opt = options.Value;
        var token = forceRefreshToken
            ? await accessTokenProvider.RefreshAccessTokenAsync(cancellationToken)
            : await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new WeChatTemplateSendResult(-1, "access_token 为空", 0);
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        var request = new WeChatTemplateMessageRequest(
            openId,
            opt.NoticeTemplateId,
            string.IsNullOrWhiteSpace(url) ? null : url,
            new WeChatNoticeTemplateData(
                new WeChatTemplateValue(TrimTitle(title, opt.TitleMaxLength)),
                new WeChatTemplateValue(createdAt.ToLocalTime().ToString("yyyy年MM月dd日 HH:mm")),
                new WeChatTemplateValue(NotificationSenderDisplayName.Resolve(senderName))));

        var response = await client.PostAsJsonAsync(
            $"/cgi-bin/message/template/send?access_token={Uri.EscapeDataString(token)}",
            request,
            JsonOptions,
            cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<WeChatTemplateSendResponse>(cancellationToken);
        if (result is null)
        {
            return new WeChatTemplateSendResult((int)response.StatusCode, "微信接口返回内容为空", 0);
        }

        return new WeChatTemplateSendResult(result.ErrCode, result.ErrMsg, result.MsgId);
    }

    /// <summary>
    /// 调用微信二维码接口。access_token 失效时由外层方法刷新后重试一次。
    /// </summary>
    private async Task<WeChatQrCodeCreateResult> CreateTemporaryQrCodeCoreAsync(
        string scene,
        int expireSeconds,
        bool forceRefreshToken,
        CancellationToken cancellationToken)
    {
        var token = forceRefreshToken
            ? await accessTokenProvider.RefreshAccessTokenAsync(cancellationToken)
            : await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new WeChatQrCodeCreateResult(-1, "access_token 为空", null, 0, null);
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        var request = new WeChatQrCodeCreateRequest(
            Math.Clamp(expireSeconds, 60, 2592000),
            "QR_STR_SCENE",
            new WeChatQrCodeActionInfo(new WeChatQrCodeScene(scene)));

        var response = await client.PostAsJsonAsync(
            $"/cgi-bin/qrcode/create?access_token={Uri.EscapeDataString(token)}",
            request,
            JsonOptions,
            cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<WeChatQrCodeCreateResponse>(cancellationToken);
        if (result is null)
        {
            return new WeChatQrCodeCreateResult((int)response.StatusCode, "微信接口返回内容为空", null, 0, null);
        }

        return string.IsNullOrWhiteSpace(result.Ticket)
            ? new WeChatQrCodeCreateResult(result.ErrCode, result.ErrMsg, null, result.ExpireSeconds, result.Url)
            : new WeChatQrCodeCreateResult(0, null, result.Ticket, result.ExpireSeconds, result.Url);
    }

    private static string TrimTitle(string title, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(title) ? "您有一条新的OA通知" : title.Trim();
        var length = maxLength > 0 ? maxLength : 20;
        return normalized.Length <= length ? normalized : normalized[..length];
    }

    private sealed record WeChatTemplateMessageRequest(
        [property: JsonPropertyName("touser")] string ToUser,
        [property: JsonPropertyName("template_id")] string TemplateId,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("data")] WeChatNoticeTemplateData Data);

    private sealed record WeChatNoticeTemplateData(
        [property: JsonPropertyName("thing8")] WeChatTemplateValue Title,
        [property: JsonPropertyName("time4")] WeChatTemplateValue Time,
        [property: JsonPropertyName("thing10")] WeChatTemplateValue Sender);

    private sealed record WeChatTemplateValue([property: JsonPropertyName("value")] string Value);

    private sealed record WeChatTemplateSendResponse(
        [property: JsonPropertyName("errcode")] int ErrCode,
        [property: JsonPropertyName("errmsg")] string? ErrMsg,
        [property: JsonPropertyName("msgid")] long MsgId);

    private sealed record WeChatQrCodeCreateRequest(
        [property: JsonPropertyName("expire_seconds")] int ExpireSeconds,
        [property: JsonPropertyName("action_name")] string ActionName,
        [property: JsonPropertyName("action_info")] WeChatQrCodeActionInfo ActionInfo);

    private sealed record WeChatQrCodeActionInfo([property: JsonPropertyName("scene")] WeChatQrCodeScene Scene);

    private sealed record WeChatQrCodeScene([property: JsonPropertyName("scene_str")] string SceneStr);

    private sealed record WeChatQrCodeCreateResponse(
        [property: JsonPropertyName("errcode")] int ErrCode,
        [property: JsonPropertyName("errmsg")] string? ErrMsg,
        [property: JsonPropertyName("ticket")] string? Ticket,
        [property: JsonPropertyName("expire_seconds")] int ExpireSeconds,
        [property: JsonPropertyName("url")] string? Url);
}
