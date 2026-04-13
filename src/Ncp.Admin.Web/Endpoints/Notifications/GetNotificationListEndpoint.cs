using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Notifications;

/// <summary>
/// 获取通知列表的请求模型
/// </summary>
/// <param name="Type">通知类型筛选</param>
/// <param name="IsRead">是否已读筛选</param>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
public record GetNotificationListRequest(
    NotificationType? Type = null,
    bool? IsRead = null,
    int PageIndex = 1,
    int PageSize = 20);

/// <summary>
/// 通知列表项
/// </summary>
/// <param name="Id">通知 ID</param>
/// <param name="Title">标题</param>
/// <param name="Content">内容</param>
/// <param name="Type">类型</param>
/// <param name="Level">级别</param>
/// <param name="SenderName">发送人名称</param>
/// <param name="IsRead">是否已读</param>
/// <param name="ReadAt">已读时间</param>
/// <param name="BusinessId">业务关联 ID</param>
/// <param name="BusinessType">业务类型</param>
/// <param name="CreatedAt">创建时间</param>
public record NotificationListItem(
    NotificationId Id, string Title, string Content,
    NotificationType Type, NotificationLevel Level,
    string SenderName, bool IsRead, DateTimeOffset? ReadAt,
    string? BusinessId, string? BusinessType, DateTimeOffset CreatedAt);

/// <summary>
/// 获取通知列表的响应模型
/// </summary>
/// <param name="Items">当前页列表</param>
/// <param name="Total">总条数</param>
/// <param name="UnreadCount">未读条数</param>
public record GetNotificationListResponse(IEnumerable<NotificationListItem> Items, int Total, int UnreadCount);

/// <summary>
/// 获取当前用户的通知列表
/// </summary>
public class GetNotificationListEndpoint(NotificationQuery notificationQuery) : Endpoint<GetNotificationListRequest, ResponseData<GetNotificationListResponse>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications").WithSummary("获取当前用户的通知列表"));
        Get("/api/notification");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.NotificationView);
    }

    public override async Task HandleAsync(GetNotificationListRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var input = new NotificationQueryInput
        {
            Type = req.Type,
            IsRead = req.IsRead,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize
        };

        var (items, total) = await notificationQuery.GetNotificationListAsync(userId.Id, input, ct);
        var unreadCount = await notificationQuery.GetUnreadCountAsync(userId.Id, ct);

        var result = items.Select(n => new NotificationListItem(
            n.Id, n.Title, n.Content, n.Type, n.Level,
            n.SenderName, n.IsRead, n.ReadAt,
            n.BusinessId, n.BusinessType, n.CreatedAt));

        await Send.OkAsync(new GetNotificationListResponse(result, total, unreadCount).AsResponseData(), cancellation: ct);
    }
}
