using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 委托工作流任务命令
/// </summary>
public record DelegateWorkflowTaskCommand(
    WorkflowInstanceId InstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    UserId DelegateToUserId,
    string DelegateToUserName,
    string Comment) : ICommand<WorkflowTaskId>;

public class DelegateWorkflowTaskCommandValidator : AbstractValidator<DelegateWorkflowTaskCommand>
{
    public DelegateWorkflowTaskCommandValidator()
    {
        RuleFor(x => x.DelegateToUserId).NotEmpty().WithMessage("委托人不能为空");
        RuleFor(x => x.DelegateToUserName).NotEmpty().WithMessage("委托人姓名不能为空");
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(500).WithMessage("委托备注必填且不能超过500字");
    }
}

/// <summary>
/// 委托工作流任务命令处理器
/// </summary>
public class DelegateWorkflowTaskCommandHandler(
    IWorkflowInstanceRepository workflowInstanceRepository,
    WorkflowTaskOperationAuthorizer taskOperationAuthorizer,
    WorkflowRuntimeRecordService runtimeRecordService) : ICommandHandler<DelegateWorkflowTaskCommand, WorkflowTaskId>
{
    public async Task<WorkflowTaskId> Handle(DelegateWorkflowTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await workflowInstanceRepository.GetWithTasksIgnoringQueryFiltersAsync(request.InstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        var operatorRoleIds = await taskOperationAuthorizer.EnsureCanOperateAsync(
            instance,
            request.TaskId,
            request.OperatorId,
            cancellationToken);
        var newTask = instance.DelegateTask(
            request.TaskId,
            request.OperatorId,
            operatorRoleIds,
            request.DelegateToUserId,
            request.DelegateToUserName,
            request.Comment);
        await runtimeRecordService.RecordTaskCreatedAsync(
            instance,
            [new WorkflowCreatedTask(
                newTask,
                new WorkflowAssigneeResult(
                    request.DelegateToUserId,
                    RoleId.Unassigned,
                    request.DelegateToUserName,
                    true,
                    WorkflowAssignmentSource.Delegated,
                    request.TaskId.ToString(),
                    WorkflowTaskVisibilityMode.ExplicitUser,
                    WorkflowTaskInitiatorDeptScopeMode.All,
                    "[]"))],
            "delegate",
            cancellationToken);

        return newTask.Id;
    }
}
