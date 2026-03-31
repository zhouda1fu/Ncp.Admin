using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 审批通过后推进流程：或签、会签、依次、抄送链路与结束判定。
/// </summary>
public class WorkflowOutgoingTaskService(
    WorkflowTreeTraverser treeTraverser,
    WorkflowAssigneeResolverQuery assigneeResolverQuery)
{
    /// <summary>
    /// 在实例上已通过某条任务后，创建后续待办或结束流程。若应等待会签/已创建下一依次任务则不再向下游推进。
    /// </summary>
    public async Task AdvanceAfterTaskApprovedAsync(
        WorkflowInstance instance,
        WorkflowTaskId approvedTaskId,
        WorkflowDefinition definition,
        CancellationToken cancellationToken)
    {
        var approvedTask = instance.Tasks.First(t => t.Id == approvedTaskId);
        var currentNode = treeTraverser.FindNodeByKey(definition.DefinitionJson, approvedTask.NodeKey);

        if (currentNode?.IsOrSign == true)
        {
            instance.CancelPendingTasksForSameNodeExcept(approvedTask.NodeKey, approvedTaskId);
        }

        if (currentNode?.IsCounterSign == true
            && !instance.AreAllCounterSignTasksApproved(approvedTask.NodeKey))
        {
            return;
        }

        if (currentNode != null
            && currentNode.Type == 1
            && currentNode.IsSequentialApproval
            && await TryCreateNextSequentialApprovalTaskAsync(instance, currentNode, cancellationToken))
        {
            return;
        }

        var nextNode = treeTraverser.FindNextTaskNode(definition.DefinitionJson, approvedTask.NodeKey, instance.Variables);

        while (nextNode != null)
        {
            var nextNodeAlreadyHasTasks = instance.Tasks.Any(t => t.NodeKey == nextNode.NodeKey);
            if (nextNodeAlreadyHasTasks)
            {
                // 同一节点的任务已创建过时直接返回，避免重复生成待办。
                return;
            }

            var ordered = await assigneeResolverQuery.ResolveOrderedAssigneesAsync(nextNode, instance, cancellationToken);
            var toCreate = WorkflowDesignerTaskHelper.SelectAssigneesForNodeEntry(nextNode, ordered);
            if (nextNode.Type == 1 && toCreate.Count == 0)
            {
                throw new KnownException("无法解析下一审批节点的处理人，请检查流程配置", ErrorCodes.WorkflowAssigneeResolutionFailed);
            }

            var taskType = nextNode.Type == 2 ? WorkflowTaskType.CarbonCopy : WorkflowTaskType.Approval;
            WorkflowDesignerTaskHelper.AddTasksToInstance(instance, nextNode, taskType, toCreate);

            if (nextNode.Type == 1)
            {
                break;
            }

            nextNode = treeTraverser.FindNextTaskNode(definition.DefinitionJson, nextNode.NodeKey, instance.Variables);
        }

        if (nextNode == null)
        {
            // 已无后续任务节点，流程进入完成态。
            instance.Complete();
        }
    }

    /// <summary>
    /// 依次审批：当前节点若仍有未审批的下一处理人，为其创建任务并返回 true。
    /// </summary>
    private async Task<bool> TryCreateNextSequentialApprovalTaskAsync(
        WorkflowInstance instance,
        DesignerNodeConfig currentNode,
        CancellationToken cancellationToken)
    {
        var ordered = await assigneeResolverQuery.ResolveOrderedAssigneesAsync(currentNode, instance, cancellationToken);
        if (ordered.Count == 0)
        {
            return false;
        }

        var approvedUserIds = instance.Tasks
            .Where(t =>
                t.NodeKey == currentNode.NodeKey
                && t.TaskType == WorkflowTaskType.Approval
                && t.Status == WorkflowTaskStatus.Approved
                && t.AssigneeId != new UserId(0))
            .Select(t => t.AssigneeId)
            .ToHashSet();

        var next = ordered.FirstOrDefault(a => a.AssigneeId != new UserId(0) && !approvedUserIds.Contains(a.AssigneeId));
        if (next == null || next.AssigneeId == new UserId(0))
        {
            return false;
        }

        instance.CreateTask(
            currentNode.NodeKey,
            currentNode.NodeName,
            WorkflowTaskType.Approval,
            next.AssigneeId,
            next.DisplayName);
        return true;
    }
}
