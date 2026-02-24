using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public record GetLeaveBalanceByUserYearRequest(UserId UserId, int Year);

public class GetLeaveBalanceByUserYearEndpoint(LeaveBalanceQuery query) : Endpoint<GetLeaveBalanceByUserYearRequest, ResponseData<List<LeaveBalanceQueryDto>>>
{
    public override void Configure()
    {
        Tags("Leave");
        Get("/api/admin/leave/balances/user/{userId}/year/{year}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveBalanceView);
    }

    public override async Task HandleAsync(GetLeaveBalanceByUserYearRequest req, CancellationToken ct)
    {
        var list = await query.GetByUserYearAsync(req.UserId, req.Year, ct);
        await Send.OkAsync(list.AsResponseData(), cancellation: ct);
    }
}
