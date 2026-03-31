using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 驳回任务命令
/// </summary>
public record RejectTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    string Comment) : ICommand;

/// <summary>
/// 驳回任务命令验证器
/// </summary>
public class RejectTaskCommandValidator : AbstractValidator<RejectTaskCommand>
{
    public RejectTaskCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.TaskId).NotNull().WithMessage("任务ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
        RuleFor(c => c.Comment).NotEmpty().WithMessage("驳回时必须填写审批意见");
    }
}

/// <summary>
/// 驳回任务命令处理器
/// </summary>
public class RejectTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    UserQuery userQuery)
    : ICommandHandler<RejectTaskCommand>
{
    public async Task Handle(RejectTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        if (instance.Status != WorkflowInstanceStatus.Running)
        {
            throw new KnownException("流程未在运行中", ErrorCodes.WorkflowInstanceNotRunning);
        }

        var task = instance.Tasks.FirstOrDefault(t => t.Id == request.TaskId)
            ?? throw new KnownException("未找到该任务", ErrorCodes.WorkflowTaskNotFound);

        if (task.AssigneeId != new UserId(0))
        {
            if (task.AssigneeId != request.OperatorId)
                throw new KnownException("无权限操作该任务", ErrorCodes.WorkflowTaskNotAssignedToOperator);
        }
        else if (task.AssigneeRoleId != new RoleId(Guid.Empty))
        {
            var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(request.OperatorId, cancellationToken);
            if (!userRoleIds.Contains(task.AssigneeRoleId))
                throw new KnownException("无权限操作该任务", ErrorCodes.WorkflowTaskNotAssignedToOperator);
        }
        else
        {
            throw new KnownException("无权限操作该任务", ErrorCodes.WorkflowTaskNotAssignedToOperator);
        }

        instance.RejectTask(request.TaskId, request.OperatorId, request.Comment);
    }
}
