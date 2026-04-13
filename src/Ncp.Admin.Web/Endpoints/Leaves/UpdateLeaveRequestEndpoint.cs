using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Web.Application.Commands.Leaves;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leaves;

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
        Description(b => b.AutoTagOverride("Leave").WithSummary("更新请假申请"));
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
