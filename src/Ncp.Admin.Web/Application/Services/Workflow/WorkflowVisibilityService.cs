using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <inheritdoc />
public class WorkflowVisibilityService(ApplicationDbContext applicationDbContext) : IWorkflowVisibilityService
{
    /// <inheritdoc />
    public IQueryable<WorkflowTaskSnapshotProjection> ApplyTaskDisplayFilter(
        IQueryable<WorkflowTaskSnapshotProjection> query,
        DataPermissionContext? dataPermission,
        UserId assigneeId,
        IReadOnlyCollection<RoleId> userRoleIds)
    {
        if (dataPermission is not { Scope: not DataScope.All })
        {
            return query;
        }

        return dataPermission.Scope switch
        {
            DataScope.Self when dataPermission.UserId != null =>
                query.Where(x => x.Snapshot.BypassDataPermission
                    || (x.Snapshot.AssigneeType == AssigneeType.User && x.Snapshot.AssigneeUserId == assigneeId)
                    || (x.Snapshot.AssigneeType == AssigneeType.Role
                        && userRoleIds.Contains(x.Snapshot.AssigneeRoleId)
                        && x.Instance.InitiatorId == dataPermission.UserId)),
            DataScope.Dept when dataPermission.DeptId != null =>
                query.Where(x => x.Snapshot.BypassDataPermission
                    || (x.Snapshot.AssigneeType == AssigneeType.User && x.Snapshot.AssigneeUserId == assigneeId)
                    || (x.Snapshot.AssigneeType == AssigneeType.Role
                        && userRoleIds.Contains(x.Snapshot.AssigneeRoleId)
                        && x.Instance.InitiatorDeptId == dataPermission.DeptId)),
            DataScope.DeptAndSub or DataScope.CustomDeptAndSub when dataPermission.AuthorizedDeptIds is { Count: > 0 } deptIds =>
                query.Where(x => x.Snapshot.BypassDataPermission
                    || (x.Snapshot.AssigneeType == AssigneeType.User && x.Snapshot.AssigneeUserId == assigneeId)
                    || (x.Snapshot.AssigneeType == AssigneeType.Role
                        && userRoleIds.Contains(x.Snapshot.AssigneeRoleId)
                        && deptIds.Contains(x.Instance.InitiatorDeptId))),
            _ => query.Where(x => x.Snapshot.BypassDataPermission
                || (x.Snapshot.AssigneeType == AssigneeType.User && x.Snapshot.AssigneeUserId == assigneeId)),
        };
    }

    /// <inheritdoc />
    public async Task<bool> CanViewInstanceDetailAsync(
        WorkflowInstance instance,
        IReadOnlyDictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>> snapshotsByTaskId,
        UserId operatorId,
        IReadOnlyCollection<RoleId> operatorRoleIds,
        CancellationToken cancellationToken = default)
    {
        if (instance.InitiatorId == operatorId
            || instance.Tasks.Any(t => t.CompletedByUserId == operatorId)
            || instance.Tasks.Any(t => TaskSnapshotsMatch(t, snapshotsByTaskId, operatorId, operatorRoleIds)))
        {
            return true;
        }

        // 管理端可见性仍走 WorkflowInstance 的全局数据权限过滤；任务/详情授权另由快照判定。
        return await applicationDbContext.WorkflowInstances.AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(i => i.Id == instance.Id, cancellationToken);
    }

    /// <inheritdoc />
    public bool CanOperateTask(
        WorkflowTask task,
        IReadOnlyDictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>> snapshotsByTaskId,
        UserId operatorId,
        IReadOnlyCollection<RoleId> operatorRoleIds)
    {
        return task.Status == WorkflowTaskStatus.Pending
            && TaskSnapshotsMatch(task, snapshotsByTaskId, operatorId, operatorRoleIds);
    }

    private static bool TaskSnapshotsMatch(
        WorkflowTask task,
        IReadOnlyDictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>> snapshotsByTaskId,
        UserId operatorId,
        IReadOnlyCollection<RoleId> operatorRoleIds)
    {
        return (task.Id != null && TaskSnapshotsMatch(task.Id, snapshotsByTaskId, operatorId, operatorRoleIds))
            || task.AssignmentSnapshots.Any(s => s.Matches(operatorId, operatorRoleIds));
    }

    private static bool TaskSnapshotsMatch(
        WorkflowTaskId taskId,
        IReadOnlyDictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>> snapshotsByTaskId,
        UserId operatorId,
        IReadOnlyCollection<RoleId> operatorRoleIds)
    {
        return snapshotsByTaskId.TryGetValue(taskId, out var snapshots)
            && snapshots.Any(s => s.Matches(operatorId, operatorRoleIds));
    }
}
