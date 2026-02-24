using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public record GetLeaveRequestRequest(LeaveRequestId Id);

public class GetLeaveRequestEndpoint(LeaveRequestQuery query) : Endpoint<GetLeaveRequestRequest, ResponseData<LeaveRequestQueryDto>>
{
    public override void Configure()
    {
        Tags("Leave");
        Get("/api/admin/leave/requests/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveRequestView);
    }

    public override async Task HandleAsync(GetLeaveRequestRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(req.Id, ct);
        if (result == null)
            await Send.NotFoundAsync(ct);
        else
            await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
