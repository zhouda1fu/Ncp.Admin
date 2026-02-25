using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectTask;

/// <summary>
/// 设置任务状态命令
/// </summary>
public record SetTaskStatusCommand(TaskId Id, Ncp.Admin.Domain.AggregatesModel.TaskAggregate.TaskStatus Status) : ICommand<bool>;

/// <summary>
/// 设置任务状态命令验证器
/// </summary>
public class SetTaskStatusCommandValidator : AbstractValidator<SetTaskStatusCommand>
{
    /// <inheritdoc />
    public SetTaskStatusCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
    }
}

/// <summary>
/// 设置任务状态命令处理器
/// </summary>
public class SetTaskStatusCommandHandler(ITaskRepository repository) : ICommandHandler<SetTaskStatusCommand, bool>
{
    /// <inheritdoc />
    public async System.Threading.Tasks.Task<bool> Handle(SetTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到任务", ErrorCodes.TaskNotFound);
        task.SetStatus(request.Status);
        return true;
    }
}
