using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicles;

/// <summary>
/// 车辆列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="PlateNumber">车牌号</param>
/// <param name="Status">状态</param>
public record GetVehicleListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? PlateNumber = null,
    VehicleStatus? Status = null);

public class GetVehicleListEndpoint(VehicleQuery query)
    : Endpoint<GetVehicleListRequest, ResponseData<PagedData<VehicleQueryDto>>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Description(b => b.AutoTagOverride("Vehicle").WithSummary("车辆列表请求"));
        Get("/api/admin/vehicles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleView);
    }

    public override async Task HandleAsync(GetVehicleListRequest req, CancellationToken ct)
    {
        var input = new VehicleQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            PlateNumber = req.PlateNumber,
            Status = req.Status,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
