using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectIndustry;

public record DeleteProjectIndustryCommand(ProjectIndustryId Id) : ICommand<bool>;

public class DeleteProjectIndustryCommandValidator : AbstractValidator<DeleteProjectIndustryCommand>
{
    public DeleteProjectIndustryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteProjectIndustryCommandHandler(IProjectIndustryRepository repository) : ICommandHandler<DeleteProjectIndustryCommand, bool>
{
    public async Task<bool> Handle(DeleteProjectIndustryCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
