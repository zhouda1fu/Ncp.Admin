using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Notification;
using System.Security.Claims;

namespace Ncp.Admin.Web.Endpoints.Notification;

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
        Description(b => b.AutoTagOverride("Notifications"));
        Put("/api/notification/read-all");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await mediator.Send(new MarkAllNotificationsReadCommand(userId), ct);
        await Send.OkAsync(new MarkAllNotificationsReadResponse(count).AsResponseData(), cancellation: ct);
    }
}
