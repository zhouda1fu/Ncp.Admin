using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectTypeModule;

public record CreateProjectTypeCommand(string Name, int SortOrder = 0) : ICommand<ProjectTypeId>;

public class CreateProjectTypeCommandValidator : AbstractValidator<CreateProjectTypeCommand>
{
    public CreateProjectTypeCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateProjectTypeCommandHandler(IProjectTypeRepository repository) : ICommandHandler<CreateProjectTypeCommand, ProjectTypeId>
{
    public async Task<ProjectTypeId> Handle(CreateProjectTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProjectType(request.Name, request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
