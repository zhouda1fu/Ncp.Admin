using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 工作流运行期记录服务：为任务补齐授权快照。
/// </summary>
public class WorkflowRuntimeRecordService
{
    /// <summary>
    /// 记录新创建任务的授权快照。
    /// </summary>
    public Task RecordTaskCreatedAsync(
        WorkflowInstance instance,
        IEnumerable<WorkflowCreatedTask> taskAssignments,
        string createdReason,
        CancellationToken cancellationToken)
    {
        // 同一个节点可能因为会签或抄送生成多条任务；按对象去重后由聚合记录运行期信息。
        var seenTasks = new HashSet<WorkflowTask>(ReferenceEqualityComparer.Instance);
        var taskList = taskAssignments
            .Where(x => seenTasks.Add(x.Task))
            .ToList();
        if (taskList.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var item in taskList)
        {
            var snapshot = CreateSnapshot(item.Task, item.Assignee, createdReason);
            instance.AttachTaskAssignmentSnapshot(item.Task, snapshot);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 根据任务分配方式生成授权快照。
    /// </summary>
    private static WorkflowTaskAssignmentSnapshot CreateSnapshot(
        WorkflowTask task,
        WorkflowAssigneeResult result,
        string createdReason)
    {
        return task.AssigneeType == AssigneeType.Role
            ? WorkflowTaskAssignmentSnapshot.ForRole(
                task.AssigneeRoleId,
                task.AssigneeName,
                result.Source,
                result.SourceRuleId,
                result.VisibilityMode,
                result.BypassDataPermissionFilter,
                result.InitiatorDeptScopeMode,
                result.InitiatorDeptScopeDeptIdsJson,
                createdReason)
            : WorkflowTaskAssignmentSnapshot.ForUser(
                task.AssigneeId,
                task.AssigneeName,
                result.Source,
                result.SourceRuleId,
                result.VisibilityMode,
                result.BypassDataPermissionFilter,
                result.InitiatorDeptScopeMode,
                result.InitiatorDeptScopeDeptIdsJson,
                createdReason);
    }
}
