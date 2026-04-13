using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Notifications;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Notifications;

/// <summary>
/// 标记所有通知为已读的响应模型
/// </summary>
public record MarkAllNotificationsReadResponse(int Count);

/// <summary>
/// 标记所有通知为已读
/// </summary>
public class MarkAllNotificationsReadEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<MarkAllNotificationsReadResponse>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications").WithSummary("标记所有通知为已读"));
        Put("/api/notification/read-all");
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

        var count = await mediator.Send(new MarkAllNotificationsReadCommand(userId.Id), ct);
        await Send.OkAsync(new MarkAllNotificationsReadResponse(count).AsResponseData(), cancellation: ct);
    }
}
