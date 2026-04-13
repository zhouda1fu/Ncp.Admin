using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 审批任务可见性策略：候选处理人需满足其自身数据权限可覆盖流程发起人范围。
/// </summary>
public class WorkflowTaskVisibilityPolicy(
    UserQuery userQuery,
    RoleQuery roleQuery,
    DeptQuery deptQuery)
{
    /// <summary>
    /// 对候选审批人列表按“任务归属 + 数据权限交集”做过滤。
    /// </summary>
    public async Task<IReadOnlyList<WorkflowAssigneeResult>> FilterAssigneesByDataPermissionAsync(
        WorkflowInstance instance,
        IReadOnlyList<WorkflowAssigneeResult> assignees,
        CancellationToken cancellationToken = default)
    {
        if (assignees.Count == 0)
        {
            return [];
        }

        var uniqueAssigneeIds = assignees
            .Where(a => a.AssigneeId != new UserId(0))
            .Select(a => a.AssigneeId)
            .Distinct()
            .ToList();
        if (uniqueAssigneeIds.Count == 0)
        {
            return assignees;
        }

        var visibilityMap = await BuildVisibilityMapAsync(
            uniqueAssigneeIds,
            instance.InitiatorId,
            instance.InitiatorDeptId,
            cancellationToken);

        return assignees
            .Where(a => a.AssigneeId == new UserId(0) || visibilityMap.GetValueOrDefault(a.AssigneeId))
            .ToList();
    }

    /// <summary>
    /// 判断指定用户基于自身数据权限是否可见该流程实例。
    /// </summary>
    public async Task<bool> CanUserAccessWorkflowByDataPermissionAsync(
        UserId userId,
        UserId initiatorId,
        DeptId initiatorDeptId,
        CancellationToken cancellationToken = default)
    {
        if (userId == new UserId(0))
        {
            return false;
        }

        var visibilityMap = await BuildVisibilityMapAsync(
            [userId],
            initiatorId,
            initiatorDeptId,
            cancellationToken);
        return visibilityMap.GetValueOrDefault(userId);
    }

    private async Task<Dictionary<UserId, bool>> BuildVisibilityMapAsync(
        IReadOnlyList<UserId> assigneeIds,
        UserId initiatorId,
        DeptId initiatorDeptId,
        CancellationToken cancellationToken)
    {
        var snapshots = await userQuery.GetDataPermissionSnapshotsByUserIdsAsync(assigneeIds, cancellationToken);
        if (snapshots.Count == 0)
        {
            return assigneeIds.ToDictionary(id => id, _ => false);
        }

        var allRoleIds = snapshots.SelectMany(s => s.RoleIds).Distinct().ToList();
        var roles = allRoleIds.Count > 0
            ? await roleQuery.GetAdminRolesForAssignmentAsync(allRoleIds, cancellationToken)
            : [];
        var roleMap = roles.ToDictionary(r => r.RoleId);

        var snapshotMap = snapshots.ToDictionary(s => s.UserId);
        var deptChildrenCache = new Dictionary<DeptId, HashSet<DeptId>>();
        var result = new Dictionary<UserId, bool>(assigneeIds.Count);
        foreach (var userId in assigneeIds)
        {
            if (!snapshotMap.TryGetValue(userId, out var snapshot))
            {
                result[userId] = false;
                continue;
            }

            var visible = false;
            foreach (var roleId in snapshot.RoleIds)
            {
                if (!roleMap.TryGetValue(roleId, out var role))
                {
                    continue;
                }

                if (await CanScopeCoverInitiatorAsync(
                        role,
                        snapshot.DeptId,
                        userId,
                        initiatorId,
                        initiatorDeptId,
                        deptChildrenCache,
                        cancellationToken))
                {
                    visible = true;
                    break;
                }
            }

            result[userId] = visible;
        }

        return result;
    }

    private async Task<bool> CanScopeCoverInitiatorAsync(
        AssignAdminUserRoleQueryDto role,
        DeptId userDeptId,
        UserId userId,
        UserId initiatorId,
        DeptId initiatorDeptId,
        Dictionary<DeptId, HashSet<DeptId>> deptChildrenCache,
        CancellationToken cancellationToken)
    {
        switch (role.DataScope)
        {
            case DataScope.All:
                return true;
            case DataScope.Self:
                return userId == initiatorId;
            case DataScope.Dept:
                return userDeptId != new DeptId(0) && userDeptId == initiatorDeptId;
            case DataScope.DeptAndSub:
                return await DeptTreeContainsAsync(userDeptId, initiatorDeptId, deptChildrenCache, cancellationToken);
            case DataScope.CustomDeptAndSub:
                foreach (var customDeptId in role.CustomDeptIds.Distinct())
                {
                    if (await DeptTreeContainsAsync(customDeptId, initiatorDeptId, deptChildrenCache, cancellationToken))
                    {
                        return true;
                    }
                }

                return false;
            default:
                return false;
        }
    }

    private async Task<bool> DeptTreeContainsAsync(
        DeptId rootDeptId,
        DeptId targetDeptId,
        Dictionary<DeptId, HashSet<DeptId>> deptChildrenCache,
        CancellationToken cancellationToken)
    {
        if (rootDeptId == new DeptId(0) || targetDeptId == new DeptId(0))
        {
            return false;
        }

        if (!deptChildrenCache.TryGetValue(rootDeptId, out var depts))
        {
            var ids = await deptQuery.GetAllChildDeptIdsAsync(rootDeptId, cancellationToken);
            depts = ids.ToHashSet();
            deptChildrenCache[rootDeptId] = depts;
        }

        return depts.Contains(targetDeptId);
    }
}
