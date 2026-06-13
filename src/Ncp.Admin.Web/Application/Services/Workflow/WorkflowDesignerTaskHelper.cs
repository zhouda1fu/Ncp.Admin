using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Application.Services.Workflow;

public sealed record WorkflowCreatedTask(WorkflowTask Task, WorkflowAssigneeResult Assignee);

/// <summary>
/// 根据运行图节点与解析出的处理人列表，决定要创建的任务条数并写入实例。
/// </summary>
public static class WorkflowDesignerTaskHelper
{
    public static IReadOnlyList<WorkflowAssigneeResult> SelectAssigneesForNodeEntry(
        WorkflowGraphNode node,
        IReadOnlyList<WorkflowAssigneeResult> ordered)
    {
        if (ordered.Count == 0)
        {
            return [];
        }

        if (node.Type != WorkflowGraphNodeType.Approval)
        {
            return ordered.ToList();
        }

        if (node.ApprovalMode is WorkflowGraphApprovalMode.All or WorkflowGraphApprovalMode.Any)
        {
            return ordered.ToList();
        }

        return ordered.Take(1).ToList();
    }

    public static IReadOnlyList<WorkflowTask> AddTasksToInstance(
        WorkflowInstance instance,
        WorkflowGraphNode node,
        WorkflowTaskType taskType,
        IReadOnlyList<WorkflowAssigneeResult> assignees)
    {
        return AddTaskAssignmentsToInstance(instance, node, taskType, assignees)
            .Select(x => x.Task)
            .ToList();
    }

    public static IReadOnlyList<WorkflowCreatedTask> AddTaskAssignmentsToInstance(
        WorkflowInstance instance,
        WorkflowGraphNode node,
        WorkflowTaskType taskType,
        IReadOnlyList<WorkflowAssigneeResult> assignees)
    {
        var created = new List<WorkflowCreatedTask>();
        foreach (var a in assignees)
        {
            if (a.AssigneeId != UserId.Unassigned)
            {
                var task = instance.CreateTask(node.NodeId, node.Name, taskType, a.AssigneeId, a.DisplayName);
                created.Add(new WorkflowCreatedTask(task, a));
            }
            else if (a.AssigneeRoleId != RoleId.Unassigned)
            {
                var task = instance.CreateTaskForRole(node.NodeId, node.Name, taskType, a.AssigneeRoleId, a.DisplayName);
                created.Add(new WorkflowCreatedTask(task, a));
            }
        }

        return created;
    }
}
