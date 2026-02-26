using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Vehicle;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

public class CreateVehicleBookingRequest
{
    public Guid VehicleId { get; set; }
    public string Purpose { get; set; } = "";
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
}

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
            new VehicleId(req.VehicleId), new UserId(uid), req.Purpose, req.StartAt, req.EndAt);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateVehicleBookingResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateVehicleBookingResponse(VehicleBookingId Id);
