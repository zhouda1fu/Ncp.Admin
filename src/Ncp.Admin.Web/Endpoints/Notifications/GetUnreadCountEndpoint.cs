using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Notifications;

/// <summary>
/// 获取未读通知数量的响应模型
/// </summary>
public record GetUnreadCountResponse(int Count);

/// <summary>
/// 获取当前用户未读通知数量
/// </summary>
public class GetUnreadCountEndpoint(NotificationQuery notificationQuery) : EndpointWithoutRequest<ResponseData<GetUnreadCountResponse>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications").WithSummary("获取当前用户未读通知数量"));
        Get("/api/notification/unread-count");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.NotificationView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var count = await notificationQuery.GetUnreadCountAsync(userId.Id, ct);
        await Send.OkAsync(new GetUnreadCountResponse(count).AsResponseData(), cancellation: ct);
    }
}
