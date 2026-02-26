using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Commands.Vehicle;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

public class UpdateVehicleRequest
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; } = "";
    public string Model { get; set; } = "";
    public string? Remark { get; set; }
}

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
        var cmd = new UpdateVehicleCommand(new VehicleId(req.Id), req.PlateNumber, req.Model, req.Remark);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
