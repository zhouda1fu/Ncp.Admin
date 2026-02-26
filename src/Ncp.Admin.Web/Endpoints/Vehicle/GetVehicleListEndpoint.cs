using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

public class GetVehicleListRequest : VehicleQueryInput { }

public class GetVehicleListEndpoint(VehicleQuery query)
    : Endpoint<GetVehicleListRequest, ResponseData<PagedData<VehicleQueryDto>>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Get("/api/admin/vehicles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleView);
    }

    public override async Task HandleAsync(GetVehicleListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
