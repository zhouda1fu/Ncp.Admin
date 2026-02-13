using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Web.Application.Commands.Notification;

namespace Ncp.Admin.Web.Endpoints.Notification;

/// <summary>
/// 删除通知
/// </summary>
public class DeleteNotificationEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications"));
        Delete("/api/notification/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new NotificationId(Route<long>("id"));
        await mediator.Send(new DeleteNotificationCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
