using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicles;

/// <summary>
/// 车辆预订列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="VehicleId">车辆 ID</param>
/// <param name="BookerId">预订人用户 ID</param>
/// <param name="Status">状态</param>
public record GetVehicleBookingListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    Guid? VehicleId = null,
    long? BookerId = null,
    VehicleBookingStatus? Status = null);

public class GetVehicleBookingListEndpoint(VehicleBookingQuery query)
    : Endpoint<GetVehicleBookingListRequest, ResponseData<PagedData<VehicleBookingQueryDto>>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Description(b => b.AutoTagOverride("Vehicle").WithSummary("车辆预订列表请求"));
        Get("/api/admin/vehicle-bookings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleBookingView);
    }

    public override async Task HandleAsync(GetVehicleBookingListRequest req, CancellationToken ct)
    {
        var input = new VehicleBookingQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            VehicleId = req.VehicleId,
            BookerId = req.BookerId,
            Status = req.Status,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
