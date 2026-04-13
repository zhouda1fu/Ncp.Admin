using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Projects;

public record RemoveProjectFollowUpRecordCommand(ProjectId ProjectId, ProjectFollowUpRecordId RecordId) : ICommand<bool>;

public class RemoveProjectFollowUpRecordCommandValidator : AbstractValidator<RemoveProjectFollowUpRecordCommand>
{
    public RemoveProjectFollowUpRecordCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.RecordId).NotEmpty();
    }
}

public class RemoveProjectFollowUpRecordCommandHandler(IProjectRepository repository) : ICommandHandler<RemoveProjectFollowUpRecordCommand, bool>
{
    public async Task<bool> Handle(RemoveProjectFollowUpRecordCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.ProjectId, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.RemoveFollowUpRecord(request.RecordId);
        return true;
    }
}
