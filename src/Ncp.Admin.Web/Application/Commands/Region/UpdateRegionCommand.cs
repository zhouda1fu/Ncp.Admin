using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Region;

public record UpdateRegionCommand(RegionId Id, string Name, RegionId ParentId, int Level, int SortOrder = 0)
    : ICommand<bool>;

public class UpdateRegionCommandValidator : AbstractValidator<UpdateRegionCommand>
{
    public UpdateRegionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Level).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRegionCommandHandler(IRegionRepository repository)
    : ICommandHandler<UpdateRegionCommand, bool>
{
    public async Task<bool> Handle(UpdateRegionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到区域", ErrorCodes.RegionNotFound);
        entity.Update(request.Name, request.ParentId, request.Level, request.SortOrder);
        return true;
    }
}
