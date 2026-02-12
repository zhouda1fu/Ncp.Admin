using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using System.Security.Claims;

namespace Ncp.Admin.Web.Endpoints.Notification;

/// <summary>
/// 获取通知列表的请求模型
/// </summary>
public class GetNotificationListRequest
{
    public NotificationType? Type { get; set; }
    public bool? IsRead { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 通知列表项
/// </summary>
public record NotificationListItem(
    NotificationId Id, string Title, string Content,
    NotificationType Type, NotificationLevel Level,
    string SenderName, bool IsRead, DateTimeOffset? ReadAt,
    string? BusinessId, string? BusinessType, DateTimeOffset CreatedAt);

/// <summary>
/// 获取通知列表的响应模型
/// </summary>
public record GetNotificationListResponse(IEnumerable<NotificationListItem> Items, int Total, int UnreadCount);

/// <summary>
/// 获取当前用户的通知列表
/// </summary>
public class GetNotificationListEndpoint(NotificationQuery notificationQuery) : Endpoint<GetNotificationListRequest, ResponseData<GetNotificationListResponse>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications"));
        Get("/api/notification");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetNotificationListRequest req, CancellationToken ct)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var input = new NotificationQueryInput
        {
            Type = req.Type,
            IsRead = req.IsRead,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize
        };

        var (items, total) = await notificationQuery.GetNotificationListAsync(userId, input, ct);
        var unreadCount = await notificationQuery.GetUnreadCountAsync(userId, ct);

        var result = items.Select(n => new NotificationListItem(
            n.Id, n.Title, n.Content, n.Type, n.Level,
            n.SenderName, n.IsRead, n.ReadAt,
            n.BusinessId, n.BusinessType, n.CreatedAt));

        await Send.OkAsync(new GetNotificationListResponse(result, total, unreadCount).AsResponseData(), cancellation: ct);
    }
}
