using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 转办任务命令
/// </summary>
public record TransferTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    UserId NewAssigneeId,
    string NewAssigneeName,
    string Comment) : ICommand;

/// <summary>
/// 转办任务命令验证器
/// </summary>
public class TransferTaskCommandValidator : AbstractValidator<TransferTaskCommand>
{
    public TransferTaskCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.TaskId).NotNull().WithMessage("任务ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
        RuleFor(c => c.NewAssigneeId).NotNull().WithMessage("新处理人ID不能为空");
        RuleFor(c => c.NewAssigneeName).NotEmpty().WithMessage("新处理人姓名不能为空");
    }
}

/// <summary>
/// 转办任务命令处理器
/// </summary>
public class TransferTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    WorkflowTaskOperationAuthorizer taskOperationAuthorizer,
    WorkflowRuntimeRecordService runtimeRecordService)
    : ICommandHandler<TransferTaskCommand>
{
    public async Task Handle(TransferTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetWithTasksIgnoringQueryFiltersAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        var operatorRoleIds = await taskOperationAuthorizer.EnsureCanOperateAsync(
            instance,
            request.TaskId,
            request.OperatorId,
            cancellationToken);
        var newTask = instance.TransferTask(
            request.TaskId,
            request.OperatorId,
            operatorRoleIds,
            request.NewAssigneeId,
            request.NewAssigneeName,
            request.Comment);
        await runtimeRecordService.RecordTaskCreatedAsync(
            instance,
            [new WorkflowCreatedTask(
                newTask,
                new WorkflowAssigneeResult(
                    request.NewAssigneeId,
                    RoleId.Unassigned,
                    request.NewAssigneeName,
                    true,
                    WorkflowAssignmentSource.Transferred,
                    request.TaskId.ToString(),
                    WorkflowTaskVisibilityMode.ExplicitUser,
                    WorkflowTaskInitiatorDeptScopeMode.All,
                    "[]"))],
            "transfer",
            cancellationToken);
    }
}
