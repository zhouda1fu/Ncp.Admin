using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Web.Application.Commands.Notification;

namespace Ncp.Admin.Web.Endpoints.Notification;

/// <summary>
/// 标记通知为已读的请求模型
/// </summary>
public record MarkNotificationReadRequest(NotificationId Id);

/// <summary>
/// 标记通知为已读
/// </summary>
public class MarkNotificationReadEndpoint(IMediator mediator) : Endpoint<MarkNotificationReadRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications"));
        Put("/api/notification/{id}/read");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(MarkNotificationReadRequest req, CancellationToken ct)
    {
        await mediator.Send(new MarkNotificationReadCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
