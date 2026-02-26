using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

public class GetVehicleBookingListRequest : VehicleBookingQueryInput { }

public class GetVehicleBookingListEndpoint(VehicleBookingQuery query)
    : Endpoint<GetVehicleBookingListRequest, ResponseData<PagedData<VehicleBookingQueryDto>>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Get("/api/admin/vehicle-bookings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleBookingView);
    }

    public override async Task HandleAsync(GetVehicleBookingListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
