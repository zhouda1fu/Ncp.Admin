using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using VehicleEntity = Ncp.Admin.Domain.AggregatesModel.VehicleAggregate.Vehicle;

namespace Ncp.Admin.Web.Application.Commands.Vehicle;

public record CreateVehicleCommand(string PlateNumber, string Model, string? Remark = null) : ICommand<VehicleId>;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(c => c.PlateNumber).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Model).NotEmpty().MaximumLength(50);
    }
}

public class CreateVehicleCommandHandler(IVehicleRepository repository) : ICommandHandler<CreateVehicleCommand, VehicleId>
{
    public async Task<VehicleId> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = new VehicleEntity(request.PlateNumber, request.Model, request.Remark);
        await repository.AddAsync(vehicle, cancellationToken);
        return vehicle.Id;
    }
}
