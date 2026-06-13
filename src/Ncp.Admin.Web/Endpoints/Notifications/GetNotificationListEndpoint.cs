using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Notifications;

/// <summary>
/// 获取通知列表的响应模型
/// </summary>
/// <param name="Items">当前页列表</param>
/// <param name="Total">总条数</param>
/// <param name="UnreadCount">未读条数（<see cref="NotificationQueryInput.IncludeUnreadCount"/> 为 false 时为 0）</param>
public record GetNotificationListResponse(
    IEnumerable<NotificationListQueryDto> Items,
    int Total,
    int UnreadCount);

/// <summary>
/// 获取当前用户的通知列表
/// </summary>
public class GetNotificationListEndpoint(NotificationQuery notificationQuery)
    : Endpoint<NotificationQueryInput, ResponseData<GetNotificationListResponse>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications").WithSummary("获取当前用户的通知列表"));
        Get("/api/notification");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.NotificationView);
    }

    public override async Task HandleAsync(NotificationQueryInput req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        req.CountTotal = true;
        var result = await notificationQuery.GetNotificationListAsync(userId, req, ct);
        await Send.OkAsync(
            new GetNotificationListResponse(result.Page.Items, result.Page.Total, result.UnreadCount).AsResponseData(),
            cancellation: ct);
    }
}
