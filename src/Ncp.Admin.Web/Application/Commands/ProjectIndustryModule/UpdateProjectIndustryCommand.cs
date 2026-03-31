using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectIndustryModule;

public record UpdateProjectIndustryCommand(ProjectIndustryId Id, string Name, int SortOrder) : ICommand<bool>;

public class UpdateProjectIndustryCommandValidator : AbstractValidator<UpdateProjectIndustryCommand>
{
    public UpdateProjectIndustryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProjectIndustryCommandHandler(IProjectIndustryRepository repository) : ICommandHandler<UpdateProjectIndustryCommand, bool>
{
    public async Task<bool> Handle(UpdateProjectIndustryCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到项目行业", ErrorCodes.ProjectIndustryNotFound);
        entity.Update(request.Name, request.SortOrder);
        return true;
    }
}
