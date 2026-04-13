using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Projects;

public record RemoveProjectContactCommand(ProjectId ProjectId, ProjectContactId ContactId) : ICommand<bool>;

public class RemoveProjectContactCommandValidator : AbstractValidator<RemoveProjectContactCommand>
{
    public RemoveProjectContactCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.ContactId).NotEmpty();
    }
}

public class RemoveProjectContactCommandHandler(IProjectRepository repository) : ICommandHandler<RemoveProjectContactCommand, bool>
{
    public async Task<bool> Handle(RemoveProjectContactCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.ProjectId, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.RemoveContact(request.ContactId);
        return true;
    }
}
