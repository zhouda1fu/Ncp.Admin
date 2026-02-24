using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public class GetLeaveRequestListRequest : LeaveRequestQueryInput { }

public class GetLeaveRequestListEndpoint(LeaveRequestQuery query) : Endpoint<GetLeaveRequestListRequest, ResponseData<PagedData<LeaveRequestQueryDto>>>
{
    public override void Configure()
    {
        Tags("Leave");
        Get("/api/admin/leave/requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveRequestView);
    }

    public override async Task HandleAsync(GetLeaveRequestListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
