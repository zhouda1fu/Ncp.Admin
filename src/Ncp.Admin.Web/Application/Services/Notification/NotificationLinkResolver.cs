using Microsoft.Extensions.Options;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Web.Application.Services.Notification;

public interface INotificationLinkResolver
{
    Task<string?> ResolveAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// 生成与前端通知中心一致的业务跳转链接（含按接收人角色解析的路径）。
/// </summary>
public class NotificationLinkResolver(
    NotificationNavigationResolver navigationResolver,
    IOptions<WeChatOfficialAccountOptions> options) : INotificationLinkResolver
{
    public async Task<string?> ResolveAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var baseUrl = options.Value.FrontendBaseUrl?.Trim();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        var nav = await navigationResolver.ResolveAsync(
            new UserId(message.ReceiverId),
            message.BusinessId,
            message.BusinessType,
            cancellationToken);

        return CombineUrl(baseUrl, nav.Path, nav.Query);
    }

    private static string CombineUrl(
        string baseUrl,
        string route,
        IReadOnlyDictionary<string, string>? query)
    {
        // 公告通知允许配置完整外链，此时直接返回外链，避免被公众号前端地址二次拼接。
        if (route.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return AppendQuery(route, query);
        }

        var path = route.StartsWith("/", StringComparison.Ordinal)
            ? $"{baseUrl.TrimEnd('/')}{route}"
            : $"{baseUrl.TrimEnd('/')}/{route}";

        return AppendQuery(path, query);
    }

    private static string AppendQuery(
        string path,
        IReadOnlyDictionary<string, string>? query)
    {
        if (query is null || query.Count == 0)
        {
            return path;
        }

        var queryString = string.Join(
            "&",
            query.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        var separator = path.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{path}{separator}{queryString}";
    }
}
