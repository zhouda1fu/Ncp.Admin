using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectTask;

/// <summary>
/// 更新任务命令
/// </summary>
public record UpdateTaskCommand(TaskId Id, string Title, string? Description, UserId? AssigneeId, DateOnly? DueDate) : ICommand<bool>;

/// <summary>
/// 更新任务命令验证器
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    /// <inheritdoc />
    public UpdateTaskCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(500);
    }
}

/// <summary>
/// 更新任务命令处理器
/// </summary>
public class UpdateTaskCommandHandler(ITaskRepository repository) : ICommandHandler<UpdateTaskCommand, bool>
{
    /// <inheritdoc />
    public async System.Threading.Tasks.Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到任务", ErrorCodes.TaskNotFound);
        task.Update(request.Title, request.Description, request.AssigneeId, request.DueDate);
        return true;
    }
}
