using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 根据设计器节点与解析出的处理人列表，决定要创建的任务条数并写入实例。
/// </summary>
public static class WorkflowDesignerTaskHelper
{
    public static IReadOnlyList<WorkflowAssigneeResult> SelectAssigneesForNodeEntry(
        DesignerNodeConfig node,
        IReadOnlyList<WorkflowAssigneeResult> ordered)
    {
        if (ordered.Count == 0)
        {
            return [];
        }

        if (node.Type != 1)
        {
            return ordered.ToList();
        }

        if (node.IsCounterSign || node.IsOrSign)
        {
            return ordered.ToList();
        }

        return ordered.Take(1).ToList();
    }

    public static void AddTasksToInstance(
        WorkflowInstance instance,
        DesignerNodeConfig node,
        WorkflowTaskType taskType,
        IReadOnlyList<WorkflowAssigneeResult> assignees)
    {
        foreach (var a in assignees)
        {
            if (a.AssigneeId != new UserId(0))
            {
                instance.CreateTask(node.NodeKey, node.NodeName, taskType, a.AssigneeId, a.DisplayName);
            }
            else if (a.AssigneeRoleId != new RoleId(Guid.Empty))
            {
                instance.CreateTaskForRole(node.NodeKey, node.NodeName, taskType, a.AssigneeRoleId, a.DisplayName);
            }
        }
    }
}
