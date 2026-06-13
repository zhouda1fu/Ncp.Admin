using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 审批通过命令
/// </summary>
public record ApproveTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    string Comment,
    /// <summary>审批动作扩展负载。通用工作流不解释该字段，由业务适配器按 BusinessType 读取。</summary>
    IReadOnlyDictionary<string, JsonElement>? ActionPayload = null) : ICommand;

/// <summary>
/// 审批通过命令验证器
/// </summary>
public class ApproveTaskCommandValidator : AbstractValidator<ApproveTaskCommand>
{
    public ApproveTaskCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.TaskId).NotNull().WithMessage("任务ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
    }
}

/// <summary>
/// 审批通过命令处理器
/// </summary>
public class ApproveTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    IWorkflowDefinitionRepository definitionRepository,
    WorkflowOutgoingTaskService outgoingTaskService,
    WorkflowTaskOperationAuthorizer taskOperationAuthorizer,
    WorkflowBusinessAdapterDispatcher businessAdapterDispatcher)
    : ICommandHandler<ApproveTaskCommand>
{
    public async Task Handle(ApproveTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetWithTasksIgnoringQueryFiltersAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        if (instance.Status != WorkflowInstanceStatus.Running)
        {
            throw new KnownException("流程未在运行中", ErrorCodes.WorkflowInstanceNotRunning);
        }

        var operatorRoleIds = await taskOperationAuthorizer.EnsureCanOperateAsync(
            instance,
            request.TaskId,
            request.OperatorId,
            cancellationToken);
        await businessAdapterDispatcher.DispatchBeforeTaskApprovedAsync(
            new WorkflowTaskActionContext(
                instance,
                request.TaskId,
                request.OperatorId,
                operatorRoleIds,
                request.ActionPayload ?? new Dictionary<string, JsonElement>()),
            cancellationToken);
        instance.ApproveTask(request.TaskId, request.OperatorId, operatorRoleIds, request.Comment);

        var definitionVersion = await definitionRepository.GetVersionAsync(instance.WorkflowDefinitionVersionId, cancellationToken)
            ?? throw new KnownException("未找到流程定义版本，无法继续审批流转", ErrorCodes.WorkflowDefinitionNotFound);

        await outgoingTaskService.AdvanceAfterTaskApprovedAsync(
            instance,
            request.TaskId,
            definitionVersion,
            cancellationToken);
    }
}
