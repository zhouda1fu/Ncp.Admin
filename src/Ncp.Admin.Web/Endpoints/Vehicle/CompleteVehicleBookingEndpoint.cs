using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Web.Application.Commands.Vehicle;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Vehicle;

public class CompleteVehicleBookingRequest
{
    public Guid Id { get; set; }
}

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
        var cmd = new CompleteVehicleBookingCommand(new VehicleBookingId(req.Id));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
