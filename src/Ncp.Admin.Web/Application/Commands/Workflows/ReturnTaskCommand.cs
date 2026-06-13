using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 退回审批任务命令。
/// </summary>
public record ReturnTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    string Comment,
    IReadOnlyList<WorkflowReturnFieldDto> ReturnFields) : ICommand;

/// <summary>
/// 退回审批任务命令验证器。
/// </summary>
public class ReturnTaskCommandValidator : AbstractValidator<ReturnTaskCommand>
{
    public ReturnTaskCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.TaskId).NotNull().WithMessage("任务ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
        RuleFor(c => c.Comment).NotEmpty().WithMessage("退回说明不能为空").MaximumLength(1000);
        RuleForEach(c => c.ReturnFields).ChildRules(field =>
        {
            field.RuleFor(x => x.Key).NotEmpty().WithMessage("退回字段key不能为空");
            field.RuleFor(x => x.Label).NotEmpty().WithMessage("退回字段名称不能为空");
        });
    }
}

/// <summary>
/// 退回审批任务命令处理器。
/// </summary>
public class ReturnTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    IWorkflowDefinitionRepository definitionRepository,
    WorkflowGraphRuntimeService graphRuntimeService,
    WorkflowTaskOperationAuthorizer taskOperationAuthorizer,
    WorkflowRuntimeRecordService runtimeRecordService,
    WorkflowBusinessAdapterDispatcher businessAdapterDispatcher,
    UserQuery userQuery)
    : ICommandHandler<ReturnTaskCommand>
{
    public async Task Handle(ReturnTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetWithTasksIgnoringQueryFiltersAsync(
                request.WorkflowInstanceId,
                cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        if (instance.Status != WorkflowInstanceStatus.Running)
        {
            throw new KnownException("流程未在运行中", ErrorCodes.WorkflowInstanceNotRunning);
        }

        var task = instance.Tasks.FirstOrDefault(t => t.Id == request.TaskId)
            ?? throw new KnownException("未找到该任务", ErrorCodes.WorkflowTaskNotFound);
        if (task.TaskType != WorkflowTaskType.Approval)
        {
            throw new KnownException("只有审批任务可以退回", ErrorCodes.WorkflowTaskNotFound);
        }

        var operatorRoleIds = await taskOperationAuthorizer.EnsureCanOperateAsync(
            instance,
            request.TaskId,
            request.OperatorId,
            cancellationToken);

        var definitionVersion = await definitionRepository.GetVersionAsync(
                instance.WorkflowDefinitionVersionId,
                cancellationToken)
            ?? throw new KnownException("未找到流程定义版本，无法执行退回", ErrorCodes.WorkflowDefinitionNotFound);
        if (string.IsNullOrWhiteSpace(definitionVersion.GraphSnapshotJson))
        {
            throw new KnownException("流程定义缺少已发布的运行图快照，无法执行退回", ErrorCodes.WorkflowDefinitionNotFound);
        }

        var previousNodeKey = graphRuntimeService.FindPreviousApprovalNodeKey(
            definitionVersion.GraphSnapshotJson,
            instance.Variables,
            task.NodeKey);

        // 退回目标只允许两类：实际路径上的上一审批节点；若当前已是首个审批节点，则回到开始节点让发起人重新处理。
        // 这里不开放任意节点退回，避免把流程退到未经过的分支或破坏条件路径的一致性。
        var returnTargetNode = string.IsNullOrWhiteSpace(previousNodeKey)
            ? graphRuntimeService.FindStartNode(definitionVersion.GraphSnapshotJson)
            : graphRuntimeService.FindNodeByKey(definitionVersion.GraphSnapshotJson, previousNodeKey);
        if (returnTargetNode == null)
        {
            throw new KnownException("未找到退回目标节点", ErrorCodes.WorkflowDefinitionNotFound);
        }

        var currentNode = graphRuntimeService.FindNodeByKey(definitionVersion.GraphSnapshotJson, task.NodeKey);

        var returnOptions = await businessAdapterDispatcher.GetReturnOptionsAsync(instance, task, currentNode, cancellationToken);
        var normalizedFields = NormalizeReturnFields(request.ReturnFields ?? [], returnOptions);

        // 退回目标处理人必须和目标节点语义一致：
        // 1. 回上一审批节点时，重新交给该节点本轮实际通过的人；
        // 2. 回开始节点时，开始节点代表发起人编辑/重新提交，因此交给流程发起人。
        var targetApprovers = await ResolveReturnTargetApproversAsync(
            instance,
            previousNodeKey,
            cancellationToken);
        if (targetApprovers.Count == 0)
        {
            throw new KnownException("退回目标节点没有可接收的处理人", ErrorCodes.WorkflowTaskNotFound);
        }

        instance.ReturnTask(request.TaskId, request.OperatorId, operatorRoleIds, request.Comment);
        var createdTasks = WorkflowDesignerTaskHelper.AddTaskAssignmentsToInstance(
            instance,
            returnTargetNode,
            WorkflowTaskType.Approval,
            targetApprovers);

        var returnContextJson = WorkflowTaskExtraData.CreateReturnContextJson(
            new WorkflowTaskReturnContextDto(
                returnOptions.FieldMode,
                returnOptions.FieldSetCode,
                normalizedFields,
                request.Comment.Trim(),
                task.NodeKey,
                currentNode?.Name ?? task.NodeName,
                returnTargetNode.NodeId,
                returnTargetNode.Name,
                DateTimeOffset.UtcNow));

        // 退回上下文挂在新生成的目标待办上，前端和业务保存校验都从这个上下文识别可编辑字段和退回说明。
        foreach (var created in createdTasks)
        {
            created.Task.SetExtraDataJson(returnContextJson);
        }

        await runtimeRecordService.RecordTaskCreatedAsync(
            instance,
            createdTasks,
            "return",
            cancellationToken);
    }

    /// <summary>
    /// 按业务适配器返回的字段白名单校验并标准化退回字段。
    /// </summary>
    private static IReadOnlyList<WorkflowReturnFieldDto> NormalizeReturnFields(
        IReadOnlyList<WorkflowReturnFieldDto> requestedFields,
        WorkflowReturnOptionsDto returnOptions)
    {
        if (!string.Equals(returnOptions.FieldMode, WorkflowReturnFieldModeNames.Required, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        if (requestedFields.Count == 0)
        {
            throw new KnownException("请选择需要修改的字段", ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
        }

        if (returnOptions.Fields.Count == 0)
        {
            throw new KnownException("当前节点未配置可选择的退回字段", ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
        }

        var allowedByKey = returnOptions.Fields.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        var result = new List<WorkflowReturnFieldDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in requestedFields)
        {
            if (!allowedByKey.TryGetValue(field.Key, out var allowed))
            {
                throw new KnownException($"退回字段「{field.Label}」不在当前业务允许范围内", ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
            }

            if (seen.Add(allowed.Key))
            {
                result.Add(allowed);
            }
        }

        return result;
    }

    /// <summary>
    /// 根据退回目标解析接收人。
    /// </summary>
    /// <remarks>
    /// 退回上一审批节点时不能重新跑审批人规则，因为角色、主管或业务变量可能已经变化；
    /// 必须使用本轮已经通过任务中的实际处理人。首个审批节点没有历史上一审批人，
    /// 此时开始节点代表发起人修改后重新提交，因此直接分配给流程发起人。
    /// </remarks>
    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveReturnTargetApproversAsync(
        WorkflowInstance instance,
        string? previousNodeKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(previousNodeKey))
        {
            // 首个审批节点退回时，退回目标是开始节点。开始节点没有审批人配置，接收人固定为流程发起人。
            return
            [
                new WorkflowAssigneeResult(
                    instance.InitiatorId,
                    RoleId.Unassigned,
                    string.IsNullOrWhiteSpace(instance.InitiatorName) ? instance.InitiatorId.ToString() : instance.InitiatorName,
                    true,
                    WorkflowAssignmentSource.Returned,
                    string.Empty,
                    WorkflowTaskVisibilityMode.ExplicitUser,
                    WorkflowTaskInitiatorDeptScopeMode.All,
                    "[]")
            ];
        }

        var userIds = instance.Tasks
            .Where(t =>
                t.NodeKey == previousNodeKey
                && t.TaskType == WorkflowTaskType.Approval
                && t.Status == WorkflowTaskStatus.Approved)
            .Select(ResolveActualApproverId)
            .Where(id => id != UserId.Unassigned)
            .Distinct()
            .ToList();

        var results = new List<WorkflowAssigneeResult>();
        foreach (var userId in userIds)
        {
            // 历史任务只保存用户 ID，重新创建待办前补齐当前显示名，保证待办列表仍能展示处理人名称。
            var user = await userQuery.GetUserByIdAsync(userId, cancellationToken);
            var displayName = user.RealName ?? user.Name ?? userId.ToString();
            results.Add(new WorkflowAssigneeResult(
                userId,
                RoleId.Unassigned,
                displayName,
                true,
                WorkflowAssignmentSource.Returned,
                previousNodeKey,
                WorkflowTaskVisibilityMode.ExplicitUser,
                WorkflowTaskInitiatorDeptScopeMode.All,
                "[]"));
        }

        return results;
    }

    /// <summary>
    /// 指定用户任务直接取 AssigneeId；角色任务取通过时记录的实际操作人。
    /// </summary>
    private static UserId ResolveActualApproverId(WorkflowTask task) =>
        task.AssigneeId != UserId.Unassigned ? task.AssigneeId : task.CompletedByUserId;
}
