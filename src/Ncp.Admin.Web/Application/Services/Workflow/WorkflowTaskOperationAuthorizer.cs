using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 统一工作流任务操作授权，以任务创建时固化的授权快照作为任务归属事实源。
/// </summary>
public class WorkflowTaskOperationAuthorizer(
    UserQuery userQuery,
    IWorkflowVisibilityService workflowVisibilityService)
{
    /// <summary>
    /// 校验当前用户是否可以操作指定待办任务，并返回用户角色集合供聚合执行兼容性校验。
    /// </summary>
    public async Task<IReadOnlyCollection<RoleId>> EnsureCanOperateAsync(
        WorkflowInstance instance,
        WorkflowTaskId taskId,
        UserId operatorId,
        CancellationToken cancellationToken = default)
    {
        var task = instance.Tasks.FirstOrDefault(t => t.Id == taskId)
            ?? throw new KnownException("未找到该任务", ErrorCodes.WorkflowTaskNotFound);

        var operatorRoleIds = await userQuery.GetRoleIdsByUserIdAsync(operatorId, cancellationToken);
        var snapshotsByTaskId = BuildSnapshotsByTaskId(instance);
        if (!workflowVisibilityService.CanOperateTask(task, snapshotsByTaskId, operatorId, operatorRoleIds))
        {
            throw new KnownException("无权限操作该任务", ErrorCodes.WorkflowTaskNotAssignedToOperator);
        }

        return operatorRoleIds;
    }

    private static IReadOnlyDictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>> BuildSnapshotsByTaskId(
        WorkflowInstance instance)
    {
        return instance.Tasks
            .Where(t => t.Id != null && t.Id != WorkflowTaskId.Unassigned)
            .ToDictionary(
                t => t.Id,
                t => (IReadOnlyList<WorkflowTaskAssignmentSnapshot>)t.AssignmentSnapshots.ToList());
    }
}
