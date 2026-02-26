using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Vehicle;

public record CreateVehicleBookingCommand(
    VehicleId VehicleId,
    UserId BookerId,
    string Purpose,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt) : ICommand<VehicleBookingId>;

public class CreateVehicleBookingCommandValidator : AbstractValidator<CreateVehicleBookingCommand>
{
    public CreateVehicleBookingCommandValidator()
    {
        RuleFor(c => c.VehicleId).NotEmpty();
        RuleFor(c => c.BookerId).NotEmpty();
        RuleFor(c => c.Purpose).NotEmpty().MaximumLength(200);
        RuleFor(c => c.EndAt).GreaterThan(c => c.StartAt);
    }
}

public class CreateVehicleBookingCommandHandler(
    IVehicleRepository vehicleRepository,
    IVehicleBookingRepository bookingRepository) : ICommandHandler<CreateVehicleBookingCommand, VehicleBookingId>
{
    public async Task<VehicleBookingId> Handle(CreateVehicleBookingCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetAsync(request.VehicleId, cancellationToken)
            ?? throw new KnownException("未找到车辆", ErrorCodes.VehicleNotFound);
        if (vehicle.Status != VehicleStatus.Available)
            throw new KnownException("车辆不可用", ErrorCodes.VehicleBookingInvalidStatus);
        var hasConflict = await bookingRepository.HasConflictAsync(
            request.VehicleId, request.StartAt, request.EndAt, null, cancellationToken);
        if (hasConflict)
            throw new KnownException("该时段车辆已被预订", ErrorCodes.VehicleBookingConflict);
        var booking = new VehicleBooking(request.VehicleId, request.BookerId, request.Purpose, request.StartAt, request.EndAt);
        await bookingRepository.AddAsync(booking, cancellationToken);
        return booking.Id;
    }
}
