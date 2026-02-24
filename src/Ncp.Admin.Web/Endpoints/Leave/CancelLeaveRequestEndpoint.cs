using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Web.Application.Commands.Leave;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public record CancelLeaveRequestRequest(LeaveRequestId Id);

public class CancelLeaveRequestEndpoint(IMediator mediator) : Endpoint<CancelLeaveRequestRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Leave");
        Post("/api/admin/leave/requests/{id}/cancel");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveRequestCancel);
    }

    public override async Task HandleAsync(CancelLeaveRequestRequest req, CancellationToken ct)
    {
        await mediator.Send(new CancelLeaveRequestCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
