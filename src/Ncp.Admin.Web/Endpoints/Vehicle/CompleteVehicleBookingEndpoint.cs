using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Commands.VehicleModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

/// <summary>
/// 完成车辆预约请求
/// </summary>
/// <param name="Id">车辆预约 ID</param>
public record CompleteVehicleBookingRequest(VehicleBookingId Id);

/// <summary>
/// 完成车辆预约
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class CompleteVehicleBookingEndpoint(IMediator mediator) : Endpoint<CompleteVehicleBookingRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Post("/api/admin/vehicle-bookings/{id}/complete");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleBookingComplete);
    }

    public override async Task HandleAsync(CompleteVehicleBookingRequest req, CancellationToken ct)
    {
        var cmd = new CompleteVehicleBookingCommand(req.Id);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
