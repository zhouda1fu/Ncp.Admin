using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 保存/发布流程定义时校验：审批「指定成员」、抄送成员须可选中有效用户；「角色」须每个已选角色下至少有一名用户。
/// </summary>
public class WorkflowDefinitionAssigneeConfigValidator(WorkflowTreeTraverser treeTraverser, UserQuery userQuery)
{
    public async Task ValidateAsync(string? definitionJson, CancellationToken cancellationToken = default)
    {
        foreach (var node in treeTraverser.EnumerateAllNodes(definitionJson))
        {
            if (node.Type == 2)
            {
                EnsureHasMemberUsers(node, isApproval: false);
            }
            else if (node.Type == 1)
            {
                switch (node.SetType)
                {
                    case 1:
                        EnsureHasMemberUsers(node, isApproval: true);
                        break;
                    case 3:
                        await EnsureEachRoleHasUsersAsync(node, cancellationToken);
                        break;
                    case 2:
                    case 5:
                        break;
                    case 4:
                    case 7:
                        throw new KnownException(
                            $"审批节点「{node.NodeName}」使用了暂不支持的审批人类型，请改为指定成员、主管、角色或发起人自己",
                            ErrorCodes.WorkflowUnsupportedAssigneeType);
                    default:
                        throw new KnownException(
                            $"审批节点「{node.NodeName}」未正确配置审批人类型",
                            ErrorCodes.WorkflowUnsupportedAssigneeType);
                }
            }
        }
    }

    private static void EnsureHasMemberUsers(DesignerNodeConfig node, bool isApproval)
    {
        if (HasAtLeastOneParsableUserId(node))
        {
            return;
        }

        var kind = isApproval ? "审批" : "抄送";
        throw new KnownException(
            $"{kind}节点「{node.NodeName}」请选择成员，且成员须为有效用户",
            ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
    }

    private static bool HasAtLeastOneParsableUserId(DesignerNodeConfig node)
    {
        var list = node.NodeAssigneeList;
        if (list == null || list.Count == 0)
        {
            return false;
        }

        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                continue;
            }

            if (long.TryParse(item.Id, out _))
            {
                return true;
            }
        }

        return false;
    }

    private async Task EnsureEachRoleHasUsersAsync(DesignerNodeConfig node, CancellationToken cancellationToken)
    {
        var list = node.NodeAssigneeList;
        if (list == null || list.Count == 0)
        {
            throw new KnownException(
                $"审批节点「{node.NodeName}」请选择审批角色",
                ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
        }

        var parsedAny = false;
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || !Guid.TryParse(item.Id, out var roleGuid))
            {
                continue;
            }

            parsedAny = true;
            var roleId = new RoleId(roleGuid);
            var users = await userQuery.GetUserAssigneesByRoleIdAsync(roleId, cancellationToken);
            if (users.Count == 0)
            {
                var roleLabel = string.IsNullOrWhiteSpace(item.Name) ? item.Id : item.Name;
                throw new KnownException(
                    $"审批节点「{node.NodeName}」中的角色「{roleLabel}」下暂无成员，请分配用户后再保存",
                    ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
            }
        }

        if (!parsedAny)
        {
            throw new KnownException(
                $"审批节点「{node.NodeName}」请选择有效的审批角色",
                ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
        }
    }
}
