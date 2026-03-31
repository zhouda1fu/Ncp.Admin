using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 工作流审批人解析结果（指定用户或指定角色，一条任务对应一条记录）。
/// 指定用户：AssigneeId 有效、AssigneeRoleId 为 Guid.Empty；指定角色：AssigneeId 为 0、AssigneeRoleId 有效。
/// </summary>
public sealed record WorkflowAssigneeResult(UserId AssigneeId, RoleId AssigneeRoleId, string DisplayName);

/// <summary>
/// 工作流审批人解析查询：根据设计器节点配置解析出处理人。
/// setType: 1=指定成员, 2=主管, 3=角色（可多角色合并用户并去重）, 5=发起人自己；抄送节点 type=2 按成员列表解析。
/// </summary>
public class WorkflowAssigneeResolverQuery(UserQuery userQuery, DeptQuery deptQuery) : IQuery
{
    /// <summary>
    /// 解析审批人。指定用户返回 (userId, Guid.Empty, name)；指定角色返回 (0, roleId, roleName)（当前实现多将角色展开为用户列表）；
    /// 主管返回 (managerId, Guid.Empty, name)。
    /// </summary>
    public async Task<WorkflowAssigneeResult?> ResolveAssigneeAsync(
        DesignerNodeConfig node,
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
        DesignerNodeConfig node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default) =>
        await ResolveOrderedAssigneesAsync(node, instance, cancellationToken);

    /// <summary>
    /// 按设计器配置解析有序处理人列表（审批、抄送节点均适用）。
    /// </summary>
    public async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveOrderedAssigneesAsync(
        DesignerNodeConfig node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default)
    {
        if (node.Type == 2)
        {
            return await ResolveUserListFromListAsync(node, cancellationToken);
        }

        return node.SetType switch
        {
            1 => await ResolveUserListFromListAsync(node, cancellationToken),
            2 => await ResolveDeptManagerListAsync(node, instance, cancellationToken),
            3 => await ResolveRoleUserListAsync(node, cancellationToken),
            5 => await ResolveInitiatorSelfAsync(instance, cancellationToken),
            4 or 7 => throw new KnownException(
                "当前不支持该审批人类型（发起人自选/连续多级主管等），请在设计器中选择指定成员、主管、角色或发起人自己",
                ErrorCodes.WorkflowUnsupportedAssigneeType),
            _ => throw new KnownException("无法识别审批人类型，请检查流程节点配置", ErrorCodes.WorkflowUnsupportedAssigneeType),
        };
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveInitiatorSelfAsync(
        WorkflowInstance instance,
        CancellationToken cancellationToken)
    {
        var userInfo = await userQuery.GetUserByIdAsync(instance.InitiatorId, cancellationToken);
        var name = userInfo?.RealName ?? userInfo?.Name ?? instance.InitiatorName;
        return [new WorkflowAssigneeResult(instance.InitiatorId, new RoleId(Guid.Empty), name)];
    }

    /// <summary>
    /// 指定成员时返回列表中所有用户（顺序与设计器一致）
    /// </summary>
    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveUserListFromListAsync(
        DesignerNodeConfig node,
        CancellationToken cancellationToken)
    {
        var list = node.NodeAssigneeList;
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
            var name = userInfo?.RealName ?? userInfo?.Name ?? item.Name ?? string.Empty;
            results.Add(new WorkflowAssigneeResult(assigneeId, new RoleId(Guid.Empty), name));
        }

        return results;
    }

    /// <summary>
    /// 设计器「选择角色」支持多角色；合并各角色下用户并按 UserId 去重（与会签/或签一致）。
    /// </summary>
    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveRoleUserListAsync(
        DesignerNodeConfig node,
        CancellationToken cancellationToken)
    {
        var assignees = node.NodeAssigneeList;
        if (assignees == null || assignees.Count == 0)
        {
            return [];
        }

        var seenUserIds = new HashSet<UserId>();
        var results = new List<WorkflowAssigneeResult>();
        var parsedRoleCount = 0;
        foreach (var item in assignees)
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
                    results.Add(new WorkflowAssigneeResult(u.Id, new RoleId(Guid.Empty), u.DisplayName));
                }
            }
        }

        if (parsedRoleCount == 0)
        {
            return [];
        }

        return results;
    }

    /// <summary>
    /// 主管：按 examineLevel 沿 ParentId 向上遍历，取第 N 级部门的主管（level=1 当前部门，level=2 父部门…）。
    /// </summary>
    private async Task<WorkflowAssigneeResult?> ResolveDeptManagerAsync(
        DesignerNodeConfig node,
        WorkflowInstance instance,
        CancellationToken cancellationToken)
    {
        var dept = await deptQuery.GetDeptByIdAsync(instance.InitiatorDeptId, cancellationToken);
        if (dept == null)
        {
            return null;
        }

        var level = node.ExamineLevel >= 1 ? node.ExamineLevel : 1;
        for (var i = 1; i < level; i++)
        {
            dept = await deptQuery.GetDeptByIdAsync(dept.ParentId, cancellationToken);
            if (dept == null)
            {
                return null;
            }
        }

        if (dept.ManagerId == new UserId(0))
        {
            return null;
        }

        var userInfo = await userQuery.GetUserByIdAsync(dept.ManagerId, cancellationToken);
        var name = userInfo?.RealName ?? userInfo?.Name ?? string.Empty;
        return new WorkflowAssigneeResult(dept.ManagerId, new RoleId(Guid.Empty), name);
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveDeptManagerListAsync(
        DesignerNodeConfig node,
        WorkflowInstance instance,
        CancellationToken cancellationToken)
    {
        var one = await ResolveDeptManagerAsync(node, instance, cancellationToken);
        return one != null ? [one] : [];
    }
}
