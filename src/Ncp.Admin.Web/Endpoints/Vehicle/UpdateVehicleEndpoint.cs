using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Commands.Vehicle;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

/// <summary>
/// 更新车辆请求
/// </summary>
/// <param name="Id">车辆 ID</param>
/// <param name="PlateNumber">车牌号</param>
/// <param name="Model">型号</param>
/// <param name="Remark">备注</param>
public record UpdateVehicleRequest(VehicleId Id, string PlateNumber, string Model, string? Remark);

/// <summary>
/// 更新车辆
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateVehicleEndpoint(IMediator mediator) : Endpoint<UpdateVehicleRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Put("/api/admin/vehicles/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleEdit);
    }

    public override async Task HandleAsync(UpdateVehicleRequest req, CancellationToken ct)
    {
        var cmd = new UpdateVehicleCommand(req.Id, req.PlateNumber, req.Model, req.Remark);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
