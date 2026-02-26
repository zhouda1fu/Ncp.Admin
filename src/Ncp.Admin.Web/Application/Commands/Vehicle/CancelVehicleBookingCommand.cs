using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Vehicle;

public record CancelVehicleBookingCommand(VehicleBookingId Id) : ICommand<bool>;

public class CancelVehicleBookingCommandHandler(IVehicleBookingRepository repository) : ICommandHandler<CancelVehicleBookingCommand, bool>
{
    public async Task<bool> Handle(CancelVehicleBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到预订", ErrorCodes.VehicleBookingNotFound);
        booking.Cancel();
        return true;
    }
}
