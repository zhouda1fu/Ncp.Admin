using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 工作流审批人解析结果（指定用户或指定角色，一条任务对应一条记录）。
/// 指定用户：AssigneeId 有效、AssigneeRoleId 为 Guid.Empty；指定角色：AssigneeId 为 0、AssigneeRoleId 有效。
/// </summary>
public sealed record WorkflowAssigneeResult(
    UserId AssigneeId,
    RoleId AssigneeRoleId,
    string DisplayName,
    bool BypassDataPermissionFilter = false,
    WorkflowAssignmentSource Source = WorkflowAssignmentSource.Member,
    string SourceRuleId = "",
    WorkflowTaskVisibilityMode VisibilityMode = WorkflowTaskVisibilityMode.ExplicitUser,
    WorkflowTaskInitiatorDeptScopeMode InitiatorDeptScopeMode = WorkflowTaskInitiatorDeptScopeMode.DataPermission,
    string InitiatorDeptScopeDeptIdsJson = "[]");

/// <summary>
/// 工作流审批人解析查询：根据运行图节点配置解析出处理人。
/// 审批节点通过 assigneeRules 配置多个审批人来源；抄送节点通过 copyRules 配置多个抄送人来源；
/// source: Member=指定成员, DeptResponsibleUser=部门负责人, DeptResponsibleUserChain=部门负责人链, Role=角色, Initiator=流程发起人。
/// </summary>
public class WorkflowAssigneeResolverQuery(
    UserQuery userQuery,
    DeptQuery deptQuery,
    ApplicationDbContext applicationDbContext,
    WorkflowGraphRuntimeService graphRuntimeService) : IQuery, IWorkflowAssigneeResolver
{
    /// <summary>
    /// 解析审批人。指定用户返回 (userId, Guid.Empty, name)；指定角色返回 (0, roleId, roleName)（当前实现多将角色展开为用户列表）；
    /// 部门负责人返回 (userId, Guid.Empty, name)（相对上一审批节点处理人，见类说明）。
    /// </summary>
    public async Task<WorkflowAssigneeResult?> ResolveAssigneeAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default)
    {
        var list = await ResolveOrderedAssigneesAsync(node, instance, cancellationToken);
        return list.Count > 0 ? list[0] : null;
    }

    /// <summary>
    /// 解析审批人列表（有序）。会签/或签/角色多人为全量；依次审批由调用方截取首条。
    /// </summary>
    public async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveAssigneesAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default) =>
        await ResolveOrderedAssigneesAsync(node, instance, cancellationToken);

    /// <summary>
    /// 按运行图配置解析有序处理人列表（审批、抄送节点均适用）。
    /// </summary>
    public async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveOrderedAssigneesAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default)
    {
        return await ResolveOrderedAssigneesInternalAsync(node, instance, graphSnapshotJson: null, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveOrderedAssigneesForDefinitionAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        string? graphSnapshotJson,
        CancellationToken cancellationToken = default)
    {
        return await ResolveOrderedAssigneesInternalAsync(node, instance, graphSnapshotJson, cancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveOrderedAssigneesInternalAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        string? graphSnapshotJson,
        CancellationToken cancellationToken)
    {
        if (node.Type == WorkflowGraphNodeType.CarbonCopy)
        {
            if (node.CopyRules is { Count: > 0 })
            {
                return await ResolveConfiguredAssigneesAsync(
                    node.CopyRules,
                    node,
                    instance,
                    graphSnapshotJson,
                    "抄送",
                    cancellationToken);
            }

            return [];
        }

        return await ResolveApprovalConfiguredAssigneesAsync(node, instance, graphSnapshotJson, cancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveApprovalConfiguredAssigneesAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        string? graphSnapshotJson,
        CancellationToken cancellationToken)
    {
        if (node.AssigneeRules == null || node.AssigneeRules.Count == 0)
        {
            return [];
        }

        return await ResolveConfiguredAssigneesAsync(
            node.AssigneeRules,
            node,
            instance,
            graphSnapshotJson,
            "审批",
            cancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveConfiguredAssigneesAsync(
        IReadOnlyList<WorkflowGraphAssigneeRule> rules,
        WorkflowGraphNode node,
        WorkflowInstance instance,
        string? graphSnapshotJson,
        string targetName,
        CancellationToken cancellationToken)
    {
        var seen = new HashSet<UserId>();
        var results = new List<WorkflowAssigneeResult>();
        foreach (var rule in rules)
        {
            IReadOnlyList<WorkflowAssigneeResult> current = rule.Source switch
            {
                WorkflowGraphAssigneeSource.Member => await ResolveUserListFromGraphOptionsAsync(
                    rule.Users,
                    WorkflowAssignmentSource.Member,
                    rule.RuleId,
                    WorkflowTaskVisibilityMode.ExplicitUser,
                    true,
                    WorkflowTaskInitiatorDeptScopeMode.All,
                    "[]",
                    cancellationToken),
                WorkflowGraphAssigneeSource.DeptResponsibleUser => await ResolveDeptResponsibleUserListAsync(
                    node,
                    instance,
                    rule,
                    rule.Level,
                    graphSnapshotJson,
                    cancellationToken),
                WorkflowGraphAssigneeSource.DeptResponsibleUserChain => await ResolveDeptResponsibleUserChainListAsync(
                    node,
                    instance,
                    rule,
                    graphSnapshotJson,
                    cancellationToken),
                WorkflowGraphAssigneeSource.Role => await ResolveRoleUserListAsync(rule, instance, cancellationToken),
                WorkflowGraphAssigneeSource.Initiator => await ResolveInitiatorSelfAsync(instance, rule, cancellationToken),
                WorkflowGraphAssigneeSource.OrderContractSigningCompanyResponsibleUser => [],
                WorkflowGraphAssigneeSource.BusinessVariable => throw new KnownException(
                    $"当前不支持该{targetName}人类型（业务变量），请在设计器中选择指定成员、部门负责人、角色或流程发起人",
                    ErrorCodes.WorkflowUnsupportedAssigneeType),
                _ => throw new KnownException($"无法识别{targetName}人类型，请检查流程节点配置", ErrorCodes.WorkflowUnsupportedAssigneeType),
            };

            foreach (var assignee in current)
            {
                if (assignee.AssigneeId == UserId.Unassigned || seen.Add(assignee.AssigneeId))
                {
                    results.Add(assignee);
                }
            }
        }

        return results;
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveInitiatorSelfAsync(
        WorkflowInstance instance,
        WorkflowGraphAssigneeRule rule,
        CancellationToken cancellationToken)
    {
        var userInfo = await userQuery.GetUserByIdAsync(instance.InitiatorId, cancellationToken);
        var name = userInfo?.RealName ?? userInfo?.Name ?? instance.InitiatorName;
        return [new WorkflowAssigneeResult(
            instance.InitiatorId,
            RoleId.Unassigned,
            name,
            true,
            WorkflowAssignmentSource.InitiatorSelf,
            rule.RuleId,
            WorkflowTaskVisibilityMode.ExplicitUser,
            WorkflowTaskInitiatorDeptScopeMode.All,
            "[]")];
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveUserListFromGraphOptionsAsync(
        IReadOnlyList<WorkflowGraphOption> list,
        WorkflowAssignmentSource source,
        string sourceRuleId,
        WorkflowTaskVisibilityMode visibilityMode,
        bool bypassDataPermissionFilter,
        WorkflowTaskInitiatorDeptScopeMode initiatorDeptScopeMode,
        string initiatorDeptScopeDeptIdsJson,
        CancellationToken cancellationToken)
    {
        if (list == null || list.Count == 0)
        {
            return [];
        }

        var results = new List<WorkflowAssigneeResult>();
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || !long.TryParse(item.Id, out var userIdValue))
            {
                continue;
            }

            var assigneeId = new UserId(userIdValue);
            var userInfo = await userQuery.GetUserByIdAsync(assigneeId, cancellationToken);
            var name = userInfo.RealName ?? userInfo.Name ?? item.Name ?? string.Empty;
            results.Add(new WorkflowAssigneeResult(
                assigneeId,
                RoleId.Unassigned,
                name,
                bypassDataPermissionFilter,
                source,
                sourceRuleId,
                visibilityMode,
                initiatorDeptScopeMode,
                initiatorDeptScopeDeptIdsJson));
        }

        return results;
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveRoleUserListAsync(
        WorkflowGraphAssigneeRule rule,
        WorkflowInstance instance,
        CancellationToken cancellationToken)
    {
        var bypass = await ShouldBypassDataPermissionFilterAsync(rule, instance.InitiatorDeptId, cancellationToken);
        return await ResolveRoleUserListAsync(
            rule.Roles,
            bypass,
            rule.RuleId,
            MapVisibilityMode(rule),
            MapInitiatorDeptScopeMode(rule.InitiatorDeptScopeMode),
            SerializeInitiatorDeptScopeDeptIds(rule.InitiatorDeptScopeDepts),
            cancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveRoleUserListAsync(
        IReadOnlyList<WorkflowGraphOption> roles,
        bool bypassDataPermissionFilter,
        string sourceRuleId,
        WorkflowTaskVisibilityMode visibilityMode,
        WorkflowTaskInitiatorDeptScopeMode initiatorDeptScopeMode,
        string initiatorDeptScopeDeptIdsJson,
        CancellationToken cancellationToken)
    {
        if (roles == null || roles.Count == 0)
        {
            return [];
        }

        var seenUserIds = new HashSet<UserId>();
        var results = new List<WorkflowAssigneeResult>();
        var parsedRoleCount = 0;
        foreach (var item in roles)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || !Guid.TryParse(item.Id, out var roleGuid))
            {
                continue;
            }

            parsedRoleCount++;
            var roleId = new RoleId(roleGuid);
            var users = await userQuery.GetUserAssigneesByRoleIdAsync(roleId, cancellationToken);
            foreach (var u in users)
            {
                if (seenUserIds.Add(u.Id))
                {
                    results.Add(new WorkflowAssigneeResult(
                        u.Id,
                        RoleId.Unassigned,
                        u.DisplayName,
                        bypassDataPermissionFilter,
                        WorkflowAssignmentSource.Role,
                        sourceRuleId,
                        visibilityMode,
                        initiatorDeptScopeMode,
                        initiatorDeptScopeDeptIdsJson));
                }
            }
        }

        if (parsedRoleCount == 0)
        {
            return [];
        }

        return results;
    }

    private async Task<bool> ShouldBypassDataPermissionFilterAsync(
        WorkflowGraphAssigneeRule rule,
        DeptId initiatorDeptId,
        CancellationToken cancellationToken)
    {
        return rule.InitiatorDeptScopeMode switch
        {
            WorkflowGraphInitiatorDeptScopeMode.DataPermission => false,
            WorkflowGraphInitiatorDeptScopeMode.All => true,
            WorkflowGraphInitiatorDeptScopeMode.SpecifiedDeptAndSub =>
                await IsInitiatorDeptInConfiguredScopeAsync(rule.InitiatorDeptScopeDepts, initiatorDeptId, cancellationToken),
            _ => false,
        };
    }

    private async Task<bool> IsInitiatorDeptInConfiguredScopeAsync(
        IReadOnlyList<WorkflowGraphOption>? deptList,
        DeptId initiatorDeptId,
        CancellationToken cancellationToken)
    {
        if (deptList == null || deptList.Count == 0 || initiatorDeptId == DeptId.Unassigned)
        {
            return false;
        }

        foreach (var item in deptList)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || !long.TryParse(item.Id, out var deptIdValue))
            {
                continue;
            }

            var rootDeptId = new DeptId(deptIdValue);
            var deptIds = await deptQuery.GetAllChildDeptIdsAsync(rootDeptId, cancellationToken);
            if (deptIds.Contains(initiatorDeptId))
            {
                return true;
            }
        }

        return false;
    }

    private static WorkflowTaskVisibilityMode MapVisibilityMode(WorkflowGraphAssigneeRule rule)
    {
        return rule.InitiatorDeptScopeMode switch
        {
            WorkflowGraphInitiatorDeptScopeMode.All => WorkflowTaskVisibilityMode.BypassDataPermission,
            WorkflowGraphInitiatorDeptScopeMode.SpecifiedDeptAndSub => WorkflowTaskVisibilityMode.RoleDataPermissionPlusConfiguredDept,
            _ => WorkflowTaskVisibilityMode.RoleDataPermission,
        };
    }

    private static WorkflowTaskInitiatorDeptScopeMode MapInitiatorDeptScopeMode(WorkflowGraphInitiatorDeptScopeMode mode)
    {
        return mode switch
        {
            WorkflowGraphInitiatorDeptScopeMode.All => WorkflowTaskInitiatorDeptScopeMode.All,
            WorkflowGraphInitiatorDeptScopeMode.SpecifiedDeptAndSub => WorkflowTaskInitiatorDeptScopeMode.SpecifiedDeptAndSub,
            _ => WorkflowTaskInitiatorDeptScopeMode.DataPermission,
        };
    }

    private static string SerializeInitiatorDeptScopeDeptIds(IReadOnlyList<WorkflowGraphOption>? deptList)
    {
        if (deptList == null || deptList.Count == 0)
        {
            return "[]";
        }

        var ids = deptList
            .Select(d => d.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToList();
        return JsonSerializer.Serialize(ids);
    }

    /// <summary>
    /// 部门负责人：按 level 沿部门 ParentId 向上遍历，取第 N 级部门负责人列表（level=1 为锚点用户所在部门，level=2 为其父部门…）。
    /// 锚点用户：在实例变量解析后的主路径上，取当前节点之前最近的审批节点上、状态为「已通过」的处理人；
    /// 若无上一审批节点、或仍无法得到用户 ID，则回退为发起人。
    /// </summary>
    public async Task<WorkflowAssigneeResult?> ResolveNthLevelDeptResponsibleUserForUserAsync(
        UserId userId,
        int level,
        CancellationToken cancellationToken,
        bool includeResignedAnchorUser = false)
    {
        var users = await ResolveNthLevelDeptResponsibleUsersForUserInternalAsync(
            userId,
            level,
            allowParentFallbackOnEmpty: false,
            cancellationToken,
            includeResignedAnchorUser);
        return users.FirstOrDefault();
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveDeptResponsibleUserListAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        WorkflowGraphAssigneeRule rule,
        int level,
        string? graphSnapshotJsonOverride,
        CancellationToken cancellationToken)
    {
        var graphSnapshotJson = string.IsNullOrWhiteSpace(graphSnapshotJsonOverride)
            ? await GetWorkflowGraphSnapshotJsonAsync(instance, cancellationToken)
            : graphSnapshotJsonOverride;
        var prevApprovalKey = graphRuntimeService.FindPreviousApprovalNodeKey(
            graphSnapshotJson,
            instance.Variables,
            node.NodeId);
        var anchorUserIds = ResolveDeptResponsibleUserAnchorUserIds(instance, prevApprovalKey);
        var examineLevel = level >= 1 ? level : 1;

        var seen = new HashSet<UserId>();
        var results = new List<WorkflowAssigneeResult>();
        foreach (var userId in anchorUserIds)
        {
            var responsibleUsers = await ResolveNthLevelDeptResponsibleUsersForUserInternalAsync(
                userId,
                examineLevel,
                allowParentFallbackOnEmpty: false,
                cancellationToken);
            foreach (var responsibleUser in responsibleUsers)
            {
                if (!seen.Add(responsibleUser.AssigneeId))
                {
                    continue;
                }

                results.Add(responsibleUser with
                {
                    BypassDataPermissionFilter = false,
                    Source = WorkflowAssignmentSource.DeptResponsibleUser,
                    SourceRuleId = rule.RuleId,
                    VisibilityMode = WorkflowTaskVisibilityMode.ExplicitUser,
                    InitiatorDeptScopeMode = WorkflowTaskInitiatorDeptScopeMode.DataPermission,
                    InitiatorDeptScopeDeptIdsJson = "[]",
                });
            }
        }

        return results;
    }

    /// <summary>
    /// 解析「部门负责人链」规则，按锚点人员向上收集整条部门负责人链。
    /// </summary>
    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveDeptResponsibleUserChainListAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        WorkflowGraphAssigneeRule rule,
        string? graphSnapshotJsonOverride,
        CancellationToken cancellationToken)
    {
        var graphSnapshotJson = string.IsNullOrWhiteSpace(graphSnapshotJsonOverride)
            ? await GetWorkflowGraphSnapshotJsonAsync(instance, cancellationToken)
            : graphSnapshotJsonOverride;
        var prevApprovalKey = graphRuntimeService.FindPreviousApprovalNodeKey(
            graphSnapshotJson,
            instance.Variables,
            node.NodeId);
        // 优先以上一审批节点实际审批人为锚点，没有历史审批人时退回到流程发起人。
        var anchorUserIds = ResolveDeptResponsibleUserAnchorUserIds(instance, prevApprovalKey);
        var excludedUserIds = ParseUserIds(rule.ExcludeUsers).ToHashSet();

        var seen = new HashSet<UserId>();
        var results = new List<WorkflowAssigneeResult>();
        foreach (var userId in anchorUserIds)
        {
            // 每个锚点独立向上遍历部门链，再统一按人员去重。
            var chain = await ResolveDeptResponsibleUserChainForUserInternalAsync(userId, cancellationToken);
            foreach (var responsibleUser in chain)
            {
                if (excludedUserIds.Contains(responsibleUser.AssigneeId))
                {
                    continue;
                }

                if (seen.Add(responsibleUser.AssigneeId))
                {
                    results.Add(responsibleUser with
                    {
                        BypassDataPermissionFilter = false,
                        Source = WorkflowAssignmentSource.DeptResponsibleUser,
                        SourceRuleId = rule.RuleId,
                        VisibilityMode = WorkflowTaskVisibilityMode.ExplicitUser,
                        InitiatorDeptScopeMode = WorkflowTaskInitiatorDeptScopeMode.DataPermission,
                        InitiatorDeptScopeDeptIdsJson = "[]",
                    });
                }
            }
        }

        // 额外指定成员在部门负责人链之后追加，但仍遵守排除名单和去重规则。
        var extraUsers = await ResolveUserListFromGraphOptionsAsync(
            rule.ExtraUsers,
            WorkflowAssignmentSource.Member,
            rule.RuleId,
            WorkflowTaskVisibilityMode.ExplicitUser,
            true,
            WorkflowTaskInitiatorDeptScopeMode.All,
            "[]",
            cancellationToken);
        foreach (var user in extraUsers)
        {
            if (!excludedUserIds.Contains(user.AssigneeId) && seen.Add(user.AssigneeId))
            {
                results.Add(user);
            }
        }

        return results;
    }

    private async Task<string?> GetWorkflowGraphSnapshotJsonAsync(
        WorkflowInstance instance,
        CancellationToken cancellationToken)
    {
        return await applicationDbContext.WorkflowDefinitionVersions.AsNoTracking()
            .Where(v => v.Id == instance.WorkflowDefinitionVersionId)
            .Select(v => v.GraphSnapshotJson)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IReadOnlyList<UserId> ResolveDeptResponsibleUserAnchorUserIds(
        WorkflowInstance instance,
        string? previousApprovalNodeKey)
    {
        if (string.IsNullOrEmpty(previousApprovalNodeKey))
        {
            return [instance.InitiatorId];
        }

        var fromTasks = instance.Tasks
            .Where(t =>
                string.Equals(t.NodeKey, previousApprovalNodeKey, StringComparison.Ordinal)
                && t.TaskType == WorkflowTaskType.Approval
                && t.Status == WorkflowTaskStatus.Approved)
            .Select(t => t.AssigneeId != UserId.Unassigned ? t.AssigneeId : t.CompletedByUserId)
            .Where(id => id != UserId.Unassigned)
            .Distinct()
            .ToList();

        return fromTasks.Count > 0 ? fromTasks : [instance.InitiatorId];
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveNthLevelDeptResponsibleUsersForUserInternalAsync(
        UserId userId,
        int level,
        bool allowParentFallbackOnEmpty,
        CancellationToken cancellationToken,
        bool includeResignedAnchorUser = false)
    {
        var userInfo = await userQuery.GetUserByIdAsync(
            userId,
            cancellationToken,
            includeResigned: includeResignedAnchorUser);

        if (userInfo.DeptId == DeptId.Unassigned)
        {
            return [];
        }

        var dept = await deptQuery.GetDeptByIdAsync(userInfo.DeptId, cancellationToken);
        if (dept == null)
        {
            return [];
        }

        for (var i = 1; i < level; i++)
        {
            dept = await deptQuery.GetDeptByIdAsync(dept.ParentId, cancellationToken);
            if (dept == null)
            {
                return [];
            }
        }

        if (allowParentFallbackOnEmpty)
        {
            for (var walk = 0; walk < 32 && dept.ResponsibleUsers.Count == 0; walk++)
            {
                if (dept.ParentId == DeptId.Unassigned || dept.ParentId == default || dept.ParentId == dept.Id)
                {
                    break;
                }

                var parent = await deptQuery.GetDeptByIdAsync(dept.ParentId, cancellationToken);
                if (parent == null)
                {
                    break;
                }

                dept = parent;
            }
        }

        return dept.ResponsibleUsers
            .OrderBy(x => x.SortOrder)
            .Select(x => new WorkflowAssigneeResult(x.UserId, RoleId.Unassigned, x.Name))
            .ToList();
    }

    /// <summary>
    /// 从指定用户所在部门开始向上查找每一级部门负责人，直到根部门或检测到环路。
    /// </summary>
    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveDeptResponsibleUserChainForUserInternalAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var userInfo = await userQuery.GetUserByIdAsync(userId, cancellationToken);
        if (userInfo == null || userInfo.DeptId == DeptId.Unassigned)
        {
            return [];
        }

        var dept = await deptQuery.GetDeptByIdAsync(userInfo.DeptId, cancellationToken);
        var results = new List<WorkflowAssigneeResult>();
        var visitedDeptIds = new HashSet<DeptId>();
        while (dept != null && visitedDeptIds.Add(dept.Id))
        {
            // 允许某一级部门未配置负责人，继续向上寻找后续负责人。
            foreach (var responsibleUser in dept.ResponsibleUsers.OrderBy(x => x.SortOrder))
            {
                results.Add(new WorkflowAssigneeResult(responsibleUser.UserId, RoleId.Unassigned, responsibleUser.Name));
            }

            if (dept.ParentId == DeptId.Unassigned || dept.ParentId == default || dept.ParentId == dept.Id)
            {
                break;
            }

            dept = await deptQuery.GetDeptByIdAsync(dept.ParentId, cancellationToken);
        }

        return results;
    }

    /// <summary>
    /// 将图配置中的人员选项解析为用户ID，忽略空值和非法ID。
    /// </summary>
    private static IEnumerable<UserId> ParseUserIds(IReadOnlyList<WorkflowGraphOption>? users)
    {
        if (users == null)
        {
            yield break;
        }

        foreach (var item in users)
        {
            if (!string.IsNullOrWhiteSpace(item.Id) && long.TryParse(item.Id, out var value) && value > 0)
            {
                yield return new UserId(value);
            }
        }
    }
}
