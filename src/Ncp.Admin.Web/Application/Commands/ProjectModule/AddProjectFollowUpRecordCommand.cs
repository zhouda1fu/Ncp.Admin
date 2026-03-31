using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectModule;

public record AddProjectFollowUpRecordCommand(
    ProjectId ProjectId,
    string Title,
    DateOnly? VisitDate,
    int ReminderIntervalDays,
    string Content,
    UserId? CreatorId) : ICommand<ProjectFollowUpRecordId>;

public class AddProjectFollowUpRecordCommandValidator : AbstractValidator<AddProjectFollowUpRecordCommand>
{
    public AddProjectFollowUpRecordCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ReminderIntervalDays).GreaterThanOrEqualTo(0);
    }
}

public class AddProjectFollowUpRecordCommandHandler(IProjectRepository repository) : ICommandHandler<AddProjectFollowUpRecordCommand, ProjectFollowUpRecordId>
{
    public async Task<ProjectFollowUpRecordId> Handle(AddProjectFollowUpRecordCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.ProjectId, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        var id = project.AddFollowUpRecord(
            request.Title ?? string.Empty, request.VisitDate, request.ReminderIntervalDays,
            request.Content ?? string.Empty, request.CreatorId);
        return id;
    }
}
