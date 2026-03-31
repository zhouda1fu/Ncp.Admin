using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectModule;

public record ActivateProjectCommand(ProjectId Id) : ICommand<bool>;

public class ActivateProjectCommandValidator : AbstractValidator<ActivateProjectCommand>
{
    public ActivateProjectCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
    }
}

public class ActivateProjectCommandHandler(IProjectRepository repository)
    : ICommandHandler<ActivateProjectCommand, bool>
{
    public async Task<bool> Handle(ActivateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.Activate();
        return true;
    }
}
