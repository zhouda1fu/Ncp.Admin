using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Vehicle;

public record CompleteVehicleBookingCommand(VehicleBookingId Id) : ICommand<bool>;

public class CompleteVehicleBookingCommandHandler(IVehicleBookingRepository repository) : ICommandHandler<CompleteVehicleBookingCommand, bool>
{
    public async Task<bool> Handle(CompleteVehicleBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到预订", ErrorCodes.VehicleBookingNotFound);
        booking.Complete();
        return true;
    }
}
