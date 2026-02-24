using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public class GetLeaveBalanceListRequest : LeaveBalanceQueryInput { }

public class GetLeaveBalanceListEndpoint(LeaveBalanceQuery query) : Endpoint<GetLeaveBalanceListRequest, ResponseData<PagedData<LeaveBalanceQueryDto>>>
{
    public override void Configure()
    {
        Tags("Leave");
        Get("/api/admin/leave/balances");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveBalanceView);
    }

    public override async Task HandleAsync(GetLeaveBalanceListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
