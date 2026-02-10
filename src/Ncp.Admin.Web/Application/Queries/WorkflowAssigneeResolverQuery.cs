using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 工作流审批人解析结果（指定用户或指定角色，一条任务对应一条记录）
/// </summary>
public sealed record WorkflowAssigneeResult(UserId? AssigneeId, RoleId? AssigneeRoleId, string DisplayName);

/// <summary>
/// 工作流审批人解析查询：根据节点配置解析出处理人。
/// 指定用户：返回该用户；指定角色：返回该角色（一条任务，待办按角色 ID 查）。
/// </summary>
public class WorkflowAssigneeResolverQuery(UserQuery userQuery, RoleQuery roleQuery) : IQuery
{
    /// <summary>
    /// 解析审批人。指定用户返回 (userId, null, name)；指定角色返回 (null, roleId, roleName)；其他返回 null。
    /// </summary>
    public async Task<WorkflowAssigneeResult?> ResolveAssigneeAsync(
        WorkflowNode node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default)
    {
        return node.AssigneeType switch
        {
            AssigneeType.User => await ResolveUserAsync(node.AssigneeValue, cancellationToken),
            AssigneeType.Role => await ResolveRoleAsync(node.AssigneeValue, cancellationToken),
            _ => null
        };
    }

    private async Task<WorkflowAssigneeResult?> ResolveUserAsync(string assigneeValue, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assigneeValue) || !long.TryParse(assigneeValue, out var userIdValue))
            return null;
        var assigneeId = new UserId(userIdValue);
        var userInfo = await userQuery.GetUserByIdAsync(assigneeId, cancellationToken);
        var name = userInfo?.RealName ?? userInfo?.Name ?? string.Empty;
        return new WorkflowAssigneeResult(assigneeId, null, name);
    }

    /// <summary>
    /// 指定角色：返回角色（一条任务，待办按角色 ID 查，该角色下所有人可见并可审批）
    /// </summary>
    private async Task<WorkflowAssigneeResult?> ResolveRoleAsync(string assigneeValue, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assigneeValue) || !Guid.TryParse(assigneeValue, out var roleGuid))
            return null;
        var roleId = new RoleId(roleGuid);
        var roleInfo = await roleQuery.GetRoleByIdAsync(roleId, cancellationToken);
        var name = roleInfo?.Name ?? string.Empty;
        return new WorkflowAssigneeResult(null, roleId, name);
    }
}
