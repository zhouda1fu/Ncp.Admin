using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Vehicle;

public record UpdateVehicleCommand(VehicleId Id, string PlateNumber, string Model, string? Remark = null) : ICommand<bool>;

public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(c => c.PlateNumber).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Model).NotEmpty().MaximumLength(50);
    }
}

public class UpdateVehicleCommandHandler(IVehicleRepository repository) : ICommandHandler<UpdateVehicleCommand, bool>
{
    public async Task<bool> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到车辆", ErrorCodes.VehicleNotFound);
        vehicle.Update(request.PlateNumber, request.Model, request.Remark);
        return true;
    }
}
