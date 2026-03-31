using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectModule;

public record ArchiveProjectCommand(ProjectId Id) : ICommand<bool>;

public class ArchiveProjectCommandValidator : AbstractValidator<ArchiveProjectCommand>
{
    public ArchiveProjectCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
    }
}

public class ArchiveProjectCommandHandler(IProjectRepository repository)
    : ICommandHandler<ArchiveProjectCommand, bool>
{
    public async Task<bool> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.Archive();
        return true;
    }
}
