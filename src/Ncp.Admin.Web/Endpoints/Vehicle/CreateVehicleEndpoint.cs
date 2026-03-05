using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Commands.Vehicle;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

/// <summary>
/// 创建车辆请求
/// </summary>
/// <param name="PlateNumber">车牌号</param>
/// <param name="Model">型号</param>
/// <param name="Remark">备注</param>
public record CreateVehicleRequest(string PlateNumber, string Model, string? Remark);

public class CreateVehicleEndpoint(IMediator mediator) : Endpoint<CreateVehicleRequest, ResponseData<CreateVehicleResponse>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Post("/api/admin/vehicles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleEdit);
    }

    public override async Task HandleAsync(CreateVehicleRequest req, CancellationToken ct)
    {
        var cmd = new CreateVehicleCommand(req.PlateNumber, req.Model, req.Remark);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateVehicleResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateVehicleResponse(VehicleId Id);
