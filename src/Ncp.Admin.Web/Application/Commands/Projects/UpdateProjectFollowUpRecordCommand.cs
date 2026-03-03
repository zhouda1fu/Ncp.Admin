using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Projects;

public record UpdateProjectFollowUpRecordCommand(
    ProjectId ProjectId,
    ProjectFollowUpRecordId RecordId,
    string Title,
    DateOnly? VisitDate,
    int ReminderIntervalDays,
    string Content) : ICommand<bool>;

public class UpdateProjectFollowUpRecordCommandValidator : AbstractValidator<UpdateProjectFollowUpRecordCommand>
{
    public UpdateProjectFollowUpRecordCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.RecordId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ReminderIntervalDays).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProjectFollowUpRecordCommandHandler(IProjectRepository repository) : ICommandHandler<UpdateProjectFollowUpRecordCommand, bool>
{
    public async Task<bool> Handle(UpdateProjectFollowUpRecordCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.ProjectId, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.UpdateFollowUpRecord(
            request.RecordId, request.Title ?? string.Empty, request.VisitDate,
            request.ReminderIntervalDays, request.Content ?? string.Empty);
        return true;
    }
}
