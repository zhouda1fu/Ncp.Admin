using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.VehicleModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

/// <summary>
/// 创建车辆预约请求
/// </summary>
/// <param name="VehicleId">车辆 ID</param>
/// <param name="Purpose">用途</param>
/// <param name="StartAt">开始时间</param>
/// <param name="EndAt">结束时间</param>
public record CreateVehicleBookingRequest(VehicleId VehicleId, string Purpose, DateTimeOffset StartAt, DateTimeOffset EndAt);

/// <summary>
/// 创建车辆预约
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class CreateVehicleBookingEndpoint(IMediator mediator) : Endpoint<CreateVehicleBookingRequest, ResponseData<CreateVehicleBookingResponse>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Post("/api/admin/vehicle-bookings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleBookingCreate);
    }

    public override async Task HandleAsync(CreateVehicleBookingRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateVehicleBookingCommand(
            req.VehicleId, new UserId(uid), req.Purpose, req.StartAt, req.EndAt);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateVehicleBookingResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateVehicleBookingResponse(VehicleBookingId Id);
