using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectIndustries;

public record CreateProjectIndustryCommand(string Name, int SortOrder = 0) : ICommand<ProjectIndustryId>;

public class CreateProjectIndustryCommandValidator : AbstractValidator<CreateProjectIndustryCommand>
{
    public CreateProjectIndustryCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateProjectIndustryCommandHandler(IProjectIndustryRepository repository) : ICommandHandler<CreateProjectIndustryCommand, ProjectIndustryId>
{
    public async Task<ProjectIndustryId> Handle(CreateProjectIndustryCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProjectIndustry(request.Name, request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
