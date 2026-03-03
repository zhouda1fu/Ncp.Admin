using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectTask;

/// <summary>
/// 创建任务命令
/// </summary>
public record CreateTaskCommand(
    ProjectId ProjectId,
    string Title,
    string? Description,
    UserId? AssigneeId,
    DateOnly? DueDate,
    int SortOrder) : ICommand<ProjectTaskId>;

/// <summary>
/// 创建任务命令验证器
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    /// <inheritdoc />
    public CreateTaskCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotNull();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(500);
    }
}

/// <summary>
/// 创建任务命令处理器
/// </summary>
public class CreateTaskCommandHandler(IProjectTaskRepository projectTaskRepository, IProjectRepository projectRepository) : ICommandHandler<CreateTaskCommand, ProjectTaskId>
{
    /// <inheritdoc />
    public async System.Threading.Tasks.Task<ProjectTaskId> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetAsync(request.ProjectId, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        var projectTask = new Ncp.Admin.Domain.AggregatesModel.TaskAggregate.ProjectTask(
            request.ProjectId,
            request.Title,
            request.Description,
            request.AssigneeId,
            request.DueDate,
            request.SortOrder);
        await projectTaskRepository.AddAsync(projectTask, cancellationToken);
        return projectTask.Id;
    }
}
