using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Region;

public record CreateRegionCommand(long Code, string Name, long ParentCode, int Level, int SortOrder = 0)
    : ICommand<RegionId>;

public class CreateRegionCommandValidator : AbstractValidator<CreateRegionCommand>
{
    public CreateRegionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Level).GreaterThanOrEqualTo(0);
    }
}

public class CreateRegionCommandHandler(IRegionRepository repository)
    : ICommandHandler<CreateRegionCommand, RegionId>
{
    public async Task<RegionId> Handle(CreateRegionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Ncp.Admin.Domain.AggregatesModel.RegionAggregate.Region(
            new RegionId(request.Code),
            request.Name,
            new RegionId(request.ParentCode),
            request.Level,
            request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
