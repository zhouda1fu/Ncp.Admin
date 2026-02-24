using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Leave;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public record SetLeaveBalanceRequest(UserId UserId, int Year, LeaveType LeaveType, decimal TotalDays);

public record SetLeaveBalanceResponse(LeaveBalanceId Id);

public class SetLeaveBalanceEndpoint(IMediator mediator) : Endpoint<SetLeaveBalanceRequest, ResponseData<SetLeaveBalanceResponse>>
{
    public override void Configure()
    {
        Tags("Leave");
        Post("/api/admin/leave/balances");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveBalanceEdit);
    }

    public override async Task HandleAsync(SetLeaveBalanceRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SetLeaveBalanceCommand(req.UserId, req.Year, req.LeaveType, req.TotalDays), ct);
        await Send.OkAsync(new SetLeaveBalanceResponse(id).AsResponseData(), cancellation: ct);
    }
}
