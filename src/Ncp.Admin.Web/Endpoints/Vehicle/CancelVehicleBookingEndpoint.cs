using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Commands.Vehicle;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

public class CancelVehicleBookingRequest
{
    public Guid Id { get; set; }
}

public class CancelVehicleBookingEndpoint(IMediator mediator) : Endpoint<CancelVehicleBookingRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Vehicle");
        Post("/api/admin/vehicle-bookings/{id}/cancel");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleBookingCancel);
    }

    public override async Task HandleAsync(CancelVehicleBookingRequest req, CancellationToken ct)
    {
        var cmd = new CancelVehicleBookingCommand(new VehicleBookingId(req.Id));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
