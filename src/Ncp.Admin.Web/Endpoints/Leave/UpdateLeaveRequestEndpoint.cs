using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Web.Application.Commands.Leave;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public record UpdateLeaveRequestRequest(
    LeaveRequestId Id,
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Days,
    string? Reason);

public class UpdateLeaveRequestEndpoint(IMediator mediator) : Endpoint<UpdateLeaveRequestRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Leave");
        Put("/api/admin/leave/requests/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveRequestEdit);
    }

    public override async Task HandleAsync(UpdateLeaveRequestRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateLeaveRequestCommand(req.Id, req.LeaveType, req.StartDate, req.EndDate, req.Days, req.Reason ?? ""), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
