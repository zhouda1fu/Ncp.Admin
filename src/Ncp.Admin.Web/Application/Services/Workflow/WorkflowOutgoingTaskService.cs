using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 审批通过后推进流程：或签、会签、依次、抄送链路与结束判定。
/// </summary>
public class WorkflowOutgoingTaskService(
    IWorkflowApprovalAssignmentService approvalAssignmentService,
    WorkflowGraphRuntimeService graphRuntimeService,
    WorkflowRuntimeRecordService? runtimeRecordService = null)
{
    /// <summary>
    /// 在实例上已通过某条任务后，创建后续待办或结束流程。若应等待会签/已创建下一依次任务则不再向下游推进。
    /// </summary>
    public async Task AdvanceAfterTaskApprovedAsync(
        WorkflowInstance instance,
        WorkflowTaskId approvedTaskId,
        WorkflowDefinitionVersion definitionVersion,
        CancellationToken cancellationToken)
    {
        var approvedTask = instance.Tasks.First(t => t.Id == approvedTaskId);
        var graphSnapshotJson = definitionVersion.GraphSnapshotJson;
        if (string.IsNullOrWhiteSpace(graphSnapshotJson))
        {
            throw new KnownException("流程定义缺少已发布的运行图快照，无法继续审批流转", ErrorCodes.WorkflowDefinitionNotFound);
        }

        var currentNode = graphRuntimeService.FindNodeByKey(graphSnapshotJson, approvedTask.NodeKey);

        if (currentNode?.ApprovalMode == WorkflowGraphApprovalMode.Any)
        {
            instance.CancelPendingTasksForSameNodeExcept(approvedTask.NodeKey, approvedTask);
        }

        if (currentNode?.ApprovalMode == WorkflowGraphApprovalMode.All
            && !instance.AreAllCounterSignTasksApproved(approvedTask.NodeKey))
        {
            return;
        }

        if (currentNode != null
            && currentNode.Type == WorkflowGraphNodeType.Approval
            && currentNode.ApprovalMode == WorkflowGraphApprovalMode.Sequential
            && await TryCreateNextSequentialApprovalTaskAsync(instance, currentNode, graphSnapshotJson, cancellationToken))
        {
            return;
        }

        var nextNode = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, approvedTask.NodeKey, instance.Variables);

        while (nextNode != null)
        {
            if (WorkflowStartAssigneeGate.IsOfficeTaskParticipantConfigNode(nextNode))
            {
                nextNode = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, nextNode.NodeId, instance.Variables);
                continue;
            }

            var nextNodeAlreadyHasPendingTask = instance.Tasks.Any(t =>
                t.NodeKey == nextNode.NodeId
                && t.Status == WorkflowTaskStatus.Pending);
            if (nextNodeAlreadyHasPendingTask)
            {
                // 只把仍处于 Pending 的任务视为“节点已创建”。
                // 退回场景会留下 Returned 历史任务；这些历史记录不能阻止发起人或上一审批人重新提交后再次生成后续待办。
                return;
            }

            var resolution = await approvalAssignmentService.ResolveForTaskCreationAsync(
                nextNode,
                instance,
                graphSnapshotJson,
                cancellationToken);
            var toCreate = WorkflowDesignerTaskHelper.SelectAssigneesForNodeEntry(nextNode, resolution.Assignees);

            var taskType = nextNode.Type == WorkflowGraphNodeType.CarbonCopy
                ? WorkflowTaskType.CarbonCopy
                : WorkflowTaskType.Approval;
            var createdTasks = WorkflowDesignerTaskHelper.AddTaskAssignmentsToInstance(instance, nextNode, taskType, toCreate);
            if (runtimeRecordService != null)
            {
                await runtimeRecordService.RecordTaskCreatedAsync(
                    instance,
                    createdTasks,
                    "advance",
                    cancellationToken);
            }

            if (nextNode.Type == WorkflowGraphNodeType.Approval && !resolution.AutoPassed)
            {
                break;
            }

            nextNode = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, nextNode.NodeId, instance.Variables);
        }

        // 已无下一节点时：若仅剩抄送/通知类待办，业务上审批已结束，应 Complete()（会取消抄送/通知待办，与末尾仅抄送链路一致，见单元测试 TailCarbonCopyOnly）。
        // 若仍有审批类待办则保持运行。
        var hasPendingApproval = instance.Tasks.Any(t =>
            t.Status == WorkflowTaskStatus.Pending && t.TaskType == WorkflowTaskType.Approval);
        if (nextNode == null && !hasPendingApproval)
        {
            instance.Complete();
        }
    }

    /// <summary>
    /// 依次审批：当前节点若仍有未审批的下一处理人，为其创建任务并返回 true。
    /// </summary>
    private async Task<bool> TryCreateNextSequentialApprovalTaskAsync(
        WorkflowInstance instance,
        WorkflowGraphNode currentNode,
        string graphSnapshotJson,
        CancellationToken cancellationToken)
    {
        var resolution = await approvalAssignmentService.ResolveForTaskCreationAsync(
            currentNode,
            instance,
            graphSnapshotJson,
            cancellationToken);
        var ordered = resolution.Assignees;
        if (ordered.Count == 0)
        {
            return false;
        }

        var approvedUserIds = instance.Tasks
            .Where(t =>
                t.NodeKey == currentNode.NodeId
                && t.TaskType == WorkflowTaskType.Approval
                && t.Status == WorkflowTaskStatus.Approved
                && t.AssigneeId != UserId.Unassigned)
            .Select(t => t.AssigneeId)
            .ToHashSet();

        var next = ordered.FirstOrDefault(a => a.AssigneeId != UserId.Unassigned && !approvedUserIds.Contains(a.AssigneeId));
        if (next == null || next.AssigneeId == UserId.Unassigned)
        {
            return false;
        }

        var createdTask = instance.CreateTask(
            currentNode.NodeId,
            currentNode.Name,
            WorkflowTaskType.Approval,
            next.AssigneeId,
            next.DisplayName);
        if (runtimeRecordService != null)
        {
            await runtimeRecordService.RecordTaskCreatedAsync(
                instance,
                [new WorkflowCreatedTask(createdTask, next)],
                "sequential",
                cancellationToken);
        }
        return true;
    }

}
