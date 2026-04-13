using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Web.Application.Commands.Notifications;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Notifications;

/// <summary>
/// 删除通知
/// </summary>
public class DeleteNotificationEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Notifications");
        Description(b => b.AutoTagOverride("Notifications").WithSummary("删除通知"));
        Delete("/api/notification/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.NotificationView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new NotificationId(Route<long>("id"));
        await mediator.Send(new DeleteNotificationCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
