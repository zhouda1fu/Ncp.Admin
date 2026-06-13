using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Application.Services.Workflow;

public sealed record WorkflowAssigneeResolutionResult(
    IReadOnlyList<WorkflowAssigneeResult> Assignees,
    IReadOnlyList<WorkflowAssigneeResult> RawAssignees,
    IReadOnlyList<WorkflowAssigneeResult> DataPermissionFilteredOut,
    bool AutoPassed,
    IReadOnlyList<string> Messages);

public interface IWorkflowApprovalAssignmentService
{
    Task<WorkflowAssigneeResolutionResult> ResolveForTaskCreationAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        string? graphSnapshotJson = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 统一处理审批节点的人员解析、数据权限过滤、空审批人策略和自审策略。
/// </summary>
public class WorkflowApprovalAssignmentService(
    IWorkflowAssigneeResolver assigneeResolver,
    WorkflowAssigneeResolverQuery assigneeResolverQuery,
    IWorkflowTaskVisibilityPolicy taskVisibilityPolicy,
    UserQuery userQuery) : IWorkflowApprovalAssignmentService
{
    public async Task<WorkflowAssigneeResolutionResult> ResolveForTaskCreationAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        string? graphSnapshotJson = null,
        CancellationToken cancellationToken = default)
    {
        var raw = string.IsNullOrWhiteSpace(graphSnapshotJson)
            ? await assigneeResolver.ResolveOrderedAssigneesAsync(node, instance, cancellationToken)
            : await assigneeResolverQuery.ResolveOrderedAssigneesForDefinitionAsync(node, instance, graphSnapshotJson, cancellationToken);
        if (node.Type != WorkflowGraphNodeType.Approval)
        {
            return new WorkflowAssigneeResolutionResult(raw, raw, [], false, []);
        }

        var filtered = await taskVisibilityPolicy.FilterAssigneesByDataPermissionAsync(instance, raw, cancellationToken);
        var filteredOut = raw
            .Where(r => r.AssigneeId != UserId.Unassigned)
            .Where(r => filtered.All(f => f.AssigneeId != r.AssigneeId))
            .ToList();

        var messages = new List<string>();
        if (filteredOut.Count > 0)
        {
            messages.Add("部分候选审批人因数据权限不足被过滤");
        }

        var afterSelf = await ApplySelfApprovalPolicyAsync(node, instance, filtered, messages, cancellationToken);
        if (afterSelf.Count > 0)
        {
            return new WorkflowAssigneeResolutionResult(afterSelf, raw, filteredOut, false, messages);
        }

        var emptyResult = await ApplyEmptyApproverPolicyAsync(node, instance, messages, cancellationToken);
        return emptyResult with
        {
            RawAssignees = raw,
            DataPermissionFilteredOut = filteredOut,
        };
    }

    public async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveConfiguredUsersAsync(
        IReadOnlyList<WorkflowGraphOption>? list,
        CancellationToken cancellationToken = default)
    {
        if (list == null || list.Count == 0)
        {
            return [];
        }

        var seen = new HashSet<UserId>();
        var results = new List<WorkflowAssigneeResult>();
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || !long.TryParse(item.Id, out var userIdValue))
            {
                continue;
            }

            var userId = new UserId(userIdValue);
            if (!seen.Add(userId))
            {
                continue;
            }

            try
            {
                var userInfo = await userQuery.GetUserByIdAsync(userId, cancellationToken);
                var name = userInfo.RealName ?? userInfo.Name ?? item.Name ?? string.Empty;
                results.Add(new WorkflowAssigneeResult(
                    userId,
                    RoleId.Unassigned,
                    name,
                    true,
                    WorkflowAssignmentSource.EmptyApproverFallback,
                    "EmptyApproverSpecifiedMembers",
                    WorkflowTaskVisibilityMode.ExplicitUser,
                    WorkflowTaskInitiatorDeptScopeMode.All,
                    "[]"));
            }
            catch (KnownException)
            {
                // 离职或不存在用户不参与运行时兜底，发布校验会尽量提前拦截无效配置。
            }
        }

        return results;
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ApplySelfApprovalPolicyAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        IReadOnlyList<WorkflowAssigneeResult> assignees,
        List<string> messages,
        CancellationToken cancellationToken)
    {
        if (assignees.Count == 0)
        {
            return assignees;
        }

        if (assignees.All(a => a.AssigneeId != instance.InitiatorId))
        {
            return assignees;
        }

        var policy = node.SelfApprovalPolicy.Mode;
        if (policy == WorkflowGraphSelfApprovalPolicyMode.Allow)
        {
            return assignees;
        }

        var withoutInitiator = assignees
            .Where(a => a.AssigneeId != instance.InitiatorId)
            .ToList();

        if (policy == WorkflowGraphSelfApprovalPolicyMode.AutoSkip)
        {
            messages.Add("提交人与审批人为同一人，已按策略自动跳过该审批人");
            return withoutInitiator;
        }

        var responsibleUser = await assigneeResolverQuery.ResolveNthLevelDeptResponsibleUserForUserAsync(
            instance.InitiatorId,
            1,
            cancellationToken);
        if (responsibleUser == null || responsibleUser.AssigneeId == instance.InitiatorId)
        {
            messages.Add("提交人与审批人为同一人，但未找到可转交的部门负责人");
            return withoutInitiator;
        }

        messages.Add("提交人与审批人为同一人，已转交部门负责人");

        var redirected = responsibleUser with
        {
            Source = WorkflowAssignmentSource.SelfApprovalRedirect,
            SourceRuleId = node.NodeId,
            VisibilityMode = WorkflowTaskVisibilityMode.ExplicitUser,
        };
        return DeduplicateUsers(withoutInitiator.Concat([redirected]));
    }

    private async Task<WorkflowAssigneeResolutionResult> ApplyEmptyApproverPolicyAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        List<string> messages,
        CancellationToken cancellationToken)
    {
        var policy = node.EmptyApproverPolicy.Mode;

        if (policy == WorkflowGraphEmptyApproverPolicyMode.AutoPass)
        {
            messages.Add("审批人为空，已按策略自动通过该节点");
            return new WorkflowAssigneeResolutionResult([], [], [], true, messages);
        }

        IReadOnlyList<WorkflowAssigneeResult> assignees = policy switch
        {
            WorkflowGraphEmptyApproverPolicyMode.SpecifiedMembers => await ResolveConfiguredUsersAsync(
                node.EmptyApproverPolicy.Users,
                cancellationToken),
            WorkflowGraphEmptyApproverPolicyMode.WorkflowAdmin => await ResolveWorkflowAdminsAsync(cancellationToken),
            _ => [],
        };

        if (assignees.Count == 0)
        {
            throw new KnownException(
                $"审批节点「{node.Name}」审批人为空，且兜底策略未解析到可用人员",
                ErrorCodes.WorkflowAssigneeResolutionFailed);
        }

        messages.Add(policy == WorkflowGraphEmptyApproverPolicyMode.SpecifiedMembers
            ? "审批人为空，已转交指定人员"
            : "审批人为空，已转交流程管理员");

        var filtered = await taskVisibilityPolicy.FilterAssigneesByDataPermissionAsync(instance, assignees, cancellationToken);
        if (filtered.Count == 0)
        {
            throw new KnownException(
                $"审批节点「{node.Name}」兜底审批人无数据权限处理当前发起人相关数据",
                ErrorCodes.WorkflowAssigneeDataPermissionDenied);
        }

        return new WorkflowAssigneeResolutionResult(filtered, [], [], false, messages);
    }

    private async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveWorkflowAdminsAsync(CancellationToken cancellationToken)
    {
        var users = await userQuery.GetUserAssigneesByPermissionCodeAsync(
            PermissionCodes.WorkflowManagement,
            cancellationToken);
        return users
            .Select(u => new WorkflowAssigneeResult(
                u.Id,
                RoleId.Unassigned,
                u.DisplayName,
                true,
                WorkflowAssignmentSource.EmptyApproverFallback,
                "EmptyApproverWorkflowAdmin",
                WorkflowTaskVisibilityMode.ExplicitUser,
                WorkflowTaskInitiatorDeptScopeMode.All,
                "[]"))
            .ToList();
    }

    private static IReadOnlyList<WorkflowAssigneeResult> DeduplicateUsers(IEnumerable<WorkflowAssigneeResult> assignees)
    {
        var seen = new HashSet<UserId>();
        var results = new List<WorkflowAssigneeResult>();
        foreach (var assignee in assignees)
        {
            if (assignee.AssigneeId == UserId.Unassigned || seen.Add(assignee.AssigneeId))
            {
                results.Add(assignee);
            }
        }

        return results;
    }
}
