using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectStatusOption;

public record CreateProjectStatusOptionCommand(string Name, string Code, int SortOrder = 0) : ICommand<ProjectStatusOptionId>;

public class CreateProjectStatusOptionCommandValidator : AbstractValidator<CreateProjectStatusOptionCommand>
{
    public CreateProjectStatusOptionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).MaximumLength(50);
    }
}

public class CreateProjectStatusOptionCommandHandler(IProjectStatusOptionRepository repository) : ICommandHandler<CreateProjectStatusOptionCommand, ProjectStatusOptionId>
{
    public async Task<ProjectStatusOptionId> Handle(CreateProjectStatusOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate.ProjectStatusOption(request.Name, request.Code ?? string.Empty, request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
