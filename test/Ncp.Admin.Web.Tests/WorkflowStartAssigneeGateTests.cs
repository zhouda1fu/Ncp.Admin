using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using NetCorePal.Extensions.Primitives;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// 发起前首个审批节点门禁：与发起流程命令处理器相同的解析 + 数据权限过滤校验。
/// </summary>
public class WorkflowStartAssigneeGateTests
{
    private static readonly WorkflowDefinitionId DefinitionId = new(Guid.Parse("22222222-2222-2222-2222-222222222222"));

    [Fact]
    public async Task EnsureFirstApprovalResolvableAsync_ApprovalWithAssigneesAfterFilter_Completes()
    {
        var graphSnapshotJson = ToGraphSnapshot("""
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}
            """);
        var gate = CreateGate(new UserAllowListVisibilityPolicy(10), ("a1", [Assignee(10)]));

        await gate.EnsureFirstApprovalResolvableAsync(
            DefinitionId,
            "测试",
            "OfficeTask",
            "标题",
            graphSnapshotJson,
            "{}",
            new UserId(99),
            "发起人",
            new DeptId(1),
            CancellationToken.None);
    }

    [Fact]
    public async Task EnsureFirstApprovalResolvableAsync_AllAssigneesFilteredOut_ThrowsDataPermissionDenied()
    {
        var graphSnapshotJson = ToGraphSnapshot("""
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}
            """);
        var gate = CreateGate(
            new UserAllowListVisibilityPolicy(/* 不允许任何实际用户 */),
            ("a1", [Assignee(10), Assignee(11)]));

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            gate.EnsureFirstApprovalResolvableAsync(
                DefinitionId,
                "测试",
                "OfficeTask",
                "标题",
                graphSnapshotJson,
                "{}",
                new UserId(99),
                "发起人",
                new DeptId(1),
                CancellationToken.None));

        Assert.Equal(ErrorCodes.WorkflowAssigneeDataPermissionDenied, ex.ErrorCode);
        Assert.Contains("用户10", ex.Message);
        Assert.Contains("用户11", ex.Message);
    }

    [Fact]
    public async Task EnsureFirstApprovalResolvableAsync_NoAssigneesResolved_ThrowsResolutionFailed()
    {
        var graphSnapshotJson = ToGraphSnapshot("""
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}
            """);
        var gate = CreateGate(new PassthroughWorkflowTaskVisibilityPolicy(), ("a1", []));

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            gate.EnsureFirstApprovalResolvableAsync(
                DefinitionId,
                "测试",
                "OfficeTask",
                "标题",
                graphSnapshotJson,
                "{}",
                new UserId(99),
                "发起人",
                new DeptId(1),
                CancellationToken.None));

        Assert.Equal(ErrorCodes.WorkflowAssigneeResolutionFailed, ex.ErrorCode);
    }

    [Fact]
    public async Task EnsureFirstApprovalResolvableAsync_LeadingCarbonCopy_SkipsToFirstApproval()
    {
        var graphSnapshotJson = ToGraphSnapshot("""
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,
               "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}}
            """);
        var gate = CreateGate(
            new UserAllowListVisibilityPolicy(10),
            ("cc1", [Assignee(5)]),
            ("a1", [Assignee(10)]));

        await gate.EnsureFirstApprovalResolvableAsync(
            DefinitionId,
            "测试",
            "OfficeTask",
            "标题",
            graphSnapshotJson,
            "{}",
            new UserId(99),
            "发起人",
            new DeptId(1),
            CancellationToken.None);
    }

    [Fact]
    public async Task EnsureFirstApprovalResolvableAsync_LeadingOfficeTaskParticipantNode_SkipsToRealApproval()
    {
        var graphSnapshotJson = ToGraphSnapshot("""
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"接收","nodeKey":"recv","type":1,"officeTaskParticipantNode":true,
               "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,
                 "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}}}
            """);
        var gate = CreateGate(
            new UserAllowListVisibilityPolicy(10),
            ("recv", [Assignee(5)]),
            ("cc1", [Assignee(6)]),
            ("a1", [Assignee(10)]));

        await gate.EnsureFirstApprovalResolvableAsync(
            DefinitionId,
            "测试",
            "OfficeTask",
            "标题",
            graphSnapshotJson,
            "{}",
            new UserId(99),
            "发起人",
            new DeptId(1),
            CancellationToken.None);
    }

    [Fact]
    public async Task ResolveFirstApprovalAssigneesAsync_LeadingOfficeTaskParticipantNode_ReturnsRealApprovalAssignees()
    {
        var graphSnapshotJson = ToGraphSnapshot("""
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"接收","nodeKey":"recv","type":1,"officeTaskParticipantNode":true,
               "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,
                 "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}}}
            """);
        var gate = CreateGate(
            ("recv", [Assignee(5)]),
            ("cc1", [Assignee(6)]),
            ("a1", [Assignee(10), Assignee(11)]));

        var instance = CreateInstance();
        var assignees = await gate.ResolveFirstApprovalAssigneesAsync(graphSnapshotJson, "{}", instance, CancellationToken.None);

        Assert.Equal(2, assignees.Count);
        Assert.Contains(assignees, a => a.AssigneeId == new UserId(10));
        Assert.Contains(assignees, a => a.AssigneeId == new UserId(11));
        Assert.DoesNotContain(assignees, a => a.AssigneeId == new UserId(5));
    }

    private static string ToGraphSnapshot(string legacyTreeJson) =>
        WorkflowTestGraphHelper.ConvertLegacyTreeToGraphSnapshot(legacyTreeJson);

    private static WorkflowInstance CreateInstance() =>
        new(
            DefinitionId,
            WorkflowDefinitionVersionId.Unassigned,
            "测试",
            string.Empty,
            "OfficeTask",
            "标题",
            new UserId(99),
            "发起人",
            new DeptId(1),
            "{}",
            string.Empty);

    private static WorkflowStartAssigneeGate CreateGate(
        IWorkflowTaskVisibilityPolicy visibility,
        params (string NodeKey, IReadOnlyList<WorkflowAssigneeResult> Assignees)[] assignees) =>
        new(
            new WorkflowGraphRuntimeService(),
            new FakeWorkflowApprovalAssignmentService(
                assignees.ToDictionary(x => x.NodeKey, x => x.Assignees),
                visibility));

    private static WorkflowStartAssigneeGate CreateGate(
        params (string NodeKey, IReadOnlyList<WorkflowAssigneeResult> Assignees)[] assignees) =>
        CreateGate(new PassthroughWorkflowTaskVisibilityPolicy(), assignees);

    private static WorkflowAssigneeResult Assignee(long userId) =>
        new(new UserId(userId), new RoleId(Guid.Empty), $"用户{userId}");

    private sealed class FakeWorkflowApprovalAssignmentService(
        IReadOnlyDictionary<string, IReadOnlyList<WorkflowAssigneeResult>> assigneesByNode,
        IWorkflowTaskVisibilityPolicy visibility) : IWorkflowApprovalAssignmentService
    {
        public async Task<WorkflowAssigneeResolutionResult> ResolveForTaskCreationAsync(
            WorkflowGraphNode node,
            WorkflowInstance instance,
            string? graphSnapshotJson = null,
            CancellationToken cancellationToken = default)
        {
            var raw = assigneesByNode.GetValueOrDefault(node.NodeId) ?? [];
            if (node.Type != WorkflowGraphNodeType.Approval)
            {
                return new WorkflowAssigneeResolutionResult(raw, raw, [], false, []);
            }

            var filtered = await visibility.FilterAssigneesByDataPermissionAsync(instance, raw, cancellationToken);
            var filteredOut = raw
                .Where(r => r.AssigneeId != UserId.Unassigned)
                .Where(r => filtered.All(f => f.AssigneeId != r.AssigneeId))
                .ToList();

            if (filtered.Count > 0)
            {
                return new WorkflowAssigneeResolutionResult(filtered, raw, filteredOut, false, []);
            }

            if (filteredOut.Count > 0)
            {
                var names = string.Join("、", filteredOut.Select(a => a.DisplayName));
                throw new KnownException(
                    $"审批节点「{node.Name}」候选审批人（{names}）无数据权限处理当前发起人相关数据",
                    ErrorCodes.WorkflowAssigneeDataPermissionDenied);
            }

            throw new KnownException(
                $"审批节点「{node.Name}」未解析到审批人",
                ErrorCodes.WorkflowAssigneeResolutionFailed);
        }
    }

    private sealed class PassthroughWorkflowTaskVisibilityPolicy : IWorkflowTaskVisibilityPolicy
    {
        public Task<IReadOnlyList<WorkflowAssigneeResult>> FilterAssigneesByDataPermissionAsync(
            WorkflowInstance instance,
            IReadOnlyList<WorkflowAssigneeResult> assignees,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(assignees);

        public Task<bool> CanUserAccessWorkflowByDataPermissionAsync(
            UserId userId,
            UserId initiatorId,
            DeptId initiatorDeptId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    /// <summary>仅保留列表中的用户 ID（用于模拟数据权限过滤）。</summary>
    private sealed class UserAllowListVisibilityPolicy : IWorkflowTaskVisibilityPolicy
    {
        private readonly HashSet<UserId> _allowed;

        public UserAllowListVisibilityPolicy(params long[] allowedUserIds) =>
            _allowed = allowedUserIds.Select(id => new UserId(id)).ToHashSet();

        public Task<IReadOnlyList<WorkflowAssigneeResult>> FilterAssigneesByDataPermissionAsync(
            WorkflowInstance instance,
            IReadOnlyList<WorkflowAssigneeResult> assignees,
            CancellationToken cancellationToken = default)
        {
            var filtered = assignees
                .Where(a => a.AssigneeId == UserId.Unassigned || _allowed.Contains(a.AssigneeId))
                .ToList();
            return Task.FromResult<IReadOnlyList<WorkflowAssigneeResult>>(filtered);
        }

        public Task<bool> CanUserAccessWorkflowByDataPermissionAsync(
            UserId userId,
            UserId initiatorId,
            DeptId initiatorDeptId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }
}
