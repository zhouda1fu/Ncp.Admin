using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Leaves;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leaves;

public record SetLeaveBalanceRequest(UserId UserId, int Year, LeaveType LeaveType, decimal TotalDays);

public record SetLeaveBalanceResponse(LeaveBalanceId Id);

public class SetLeaveBalanceEndpoint(IMediator mediator) : Endpoint<SetLeaveBalanceRequest, ResponseData<SetLeaveBalanceResponse>>
{
    public override void Configure()
    {
        Tags("Leave");
        Description(b => b.AutoTagOverride("Leave").WithSummary("设置假期余额"));
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
