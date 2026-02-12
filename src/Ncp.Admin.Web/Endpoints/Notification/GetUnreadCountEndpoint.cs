using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using System.Security.Claims;

namespace Ncp.Admin.Web.Endpoints.Notification;

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
        Description(b => b.AutoTagOverride("Notifications"));
        Get("/api/notification/unread-count");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await notificationQuery.GetUnreadCountAsync(userId, ct);
        await Send.OkAsync(new GetUnreadCountResponse(count).AsResponseData(), cancellation: ct);
    }
}
