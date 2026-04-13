using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Web.Application.Commands.Notifications;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Notifications;

/// <summary>
/// 标记通知为已读
/// </summary>
public class MarkNotificationReadEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications").WithSummary("标记通知为已读"));
        Put("/api/notification/{id}/read");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.NotificationView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new NotificationId(Route<long>("id"));
        await mediator.Send(new MarkNotificationReadCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
