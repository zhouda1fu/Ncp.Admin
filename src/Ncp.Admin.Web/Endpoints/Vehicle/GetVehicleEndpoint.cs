using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

public record GetVehicleRequest(VehicleId Id);

public class GetVehicleEndpoint(VehicleQuery query) : Endpoint<GetVehicleRequest, ResponseData<VehicleQueryDto>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Get("/api/admin/vehicles/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleView);
    }

    public override async Task HandleAsync(GetVehicleRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(req.Id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
