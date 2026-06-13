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
using System.Text.Json;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// 流程推进服务测试：保护或签、会签、依次审批、连续抄送和完成判定等关键流转规则。
/// </summary>
public class WorkflowOutgoingTaskServiceTests
{
    private static readonly WorkflowDefinitionId DefinitionId = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    [Fact]
    public void ConvertLegacyTree_OrSignNode_HasAnyApprovalMode()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"或签审批","nodeKey":"a1","type":1,"examineMode":3,
               "childNode":{"nodeName":"下一审批","nodeKey":"a2","type":1,"childNode":null}}}
            """;
        var snapshot = WorkflowTestGraphHelper.ConvertLegacyTreeToGraphSnapshot(json);
        var node = new WorkflowGraphRuntimeService().FindNodeByKey(snapshot, "a1");
        Assert.NotNull(node);
        Assert.Equal(WorkflowGraphApprovalMode.Any, node!.ApprovalMode);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_OrSign_CancelsOtherPendingTasksAndCreatesNextNode()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"或签审批","nodeKey":"a1","type":1,"examineMode":3,
               "childNode":{"nodeName":"下一审批","nodeKey":"a2","type":1,"childNode":null}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "或签审批", WorkflowTaskType.Approval, new UserId(1), "用户1");
        var sibling = instance.CreateTask("a1", "或签审批", WorkflowTaskType.Approval, new UserId(2), "用户2");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("a2", [Assignee(3)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Equal(WorkflowTaskStatus.Cancelled, sibling.Status);
        Assert.Contains(instance.Tasks, t => t.NodeKey == "a2" && t.AssigneeId == new UserId(3));
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_CounterSign_WaitsUntilAllCurrentNodeTasksApproved()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"会签审批","nodeKey":"a1","type":1,"examineMode":2,
               "childNode":{"nodeName":"下一审批","nodeKey":"a2","type":1,"childNode":null}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "会签审批", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.CreateTask("a1", "会签审批", WorkflowTaskType.Approval, new UserId(2), "用户2");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("a2", [Assignee(3)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.DoesNotContain(instance.Tasks, t => t.NodeKey == "a2");
        Assert.Equal(WorkflowInstanceStatus.Running, instance.Status);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_SequentialApproval_CreatesOnlyNextAssigneeForSameNode()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"依次审批","nodeKey":"a1","type":1,"examineMode":1,
               "childNode":{"nodeName":"下一审批","nodeKey":"a2","type":1,"childNode":null}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "依次审批", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("a1", [Assignee(1), Assignee(2)]), ("a2", [Assignee(3)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Contains(instance.Tasks, t => t.NodeKey == "a1" && t.AssigneeId == new UserId(2));
        Assert.DoesNotContain(instance.Tasks, t => t.NodeKey == "a2");
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_ContinuousCarbonCopyNodes_CreatesCopyTasksAndNextApproval()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,
               "childNode":{"nodeName":"抄送1","nodeKey":"cc1","type":2,
                 "childNode":{"nodeName":"抄送2","nodeKey":"cc2","type":2,
                   "childNode":{"nodeName":"审批2","nodeKey":"a2","type":1,"childNode":null}}}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("cc1", [Assignee(2)]), ("cc2", [Assignee(3)]), ("a2", [Assignee(4)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Contains(instance.Tasks, t => t.NodeKey == "cc1" && t.TaskType == WorkflowTaskType.CarbonCopy);
        Assert.Contains(instance.Tasks, t => t.NodeKey == "cc2" && t.TaskType == WorkflowTaskType.CarbonCopy);
        Assert.Contains(instance.Tasks, t => t.NodeKey == "a2" && t.TaskType == WorkflowTaskType.Approval);
        Assert.Equal(WorkflowInstanceStatus.Running, instance.Status);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_ReturnedNextNode_CreatesNewPendingTask()
    {
        // 场景：审批2退回审批1后，审批1重新通过。
        // 断言：审批2的历史 Returned 任务只作为记录保留，不能阻止重新创建审批2的新 Pending 待办。
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,
               "childNode":{"nodeName":"审批2","nodeKey":"a2","type":1,"childNode":null}}}
            """;
        var instance = CreateInstance();
        var firstPass = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        firstPass.Approve("同意", new UserId(1));
        var returnedNext = instance.CreateTask("a2", "审批2", WorkflowTaskType.Approval, new UserId(2), "用户2");
        returnedNext.Return("退回", new UserId(2));
        var resubmit = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        resubmit.Approve("重新提交", new UserId(1));

        var service = CreateService(("a2", [Assignee(2)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, resubmit.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Equal(WorkflowTaskStatus.Returned, returnedNext.Status);
        Assert.Equal(2, instance.Tasks.Count(t => t.NodeKey == "a2"));
        Assert.Contains(instance.Tasks, t =>
            t.NodeKey == "a2"
            && t.Status == WorkflowTaskStatus.Pending
            && t.AssigneeId == new UserId(2));
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_ReturnedToStart_CreatesFirstApprovalAgain()
    {
        // 场景：首个审批节点退回发起人后，发起人在开始节点对应待办上重新提交。
        // 断言：首个审批节点的历史 Returned 任务不会被当成仍有效的待办，流程会重新创建首审节点。
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,"childNode":null}}
            """;
        var instance = CreateInstance();
        var returnedFirst = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        returnedFirst.Return("退回发起人", new UserId(1));
        var initiatorTask = instance.CreateTask("root", "发起人", WorkflowTaskType.Approval, new UserId(99), "发起人");
        initiatorTask.Approve("重新提交", new UserId(99));

        var service = CreateService(("a1", [Assignee(1)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, initiatorTask.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Equal(WorkflowTaskStatus.Returned, returnedFirst.Status);
        Assert.Equal(2, instance.Tasks.Count(t => t.NodeKey == "a1"));
        Assert.Contains(instance.Tasks, t =>
            t.NodeKey == "a1"
            && t.Status == WorkflowTaskStatus.Pending
            && t.AssigneeId == new UserId(1));
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_ApprovalInsideConditionBranch_CreatesMergeApproval()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"条件路由","nodeKey":"route1","type":4,
               "conditionNodes":[
                 {"nodeName":"领导分支","nodeKey":"branch1","type":3,"priorityLevel":1,
                  "conditionList":[[{"field":"type","operator":"==","value":"leader"}]],
                  "childNode":{"nodeName":"领导审核","nodeKey":"leader","type":1,"childNode":null}},
                 {"nodeName":"其他情况","nodeKey":"branch2","type":3,"priorityLevel":2,
                  "conditionList":[],
                  "childNode":{"nodeName":"普通审核","nodeKey":"normal","type":1,"childNode":null}}
               ],
               "childNode":{"nodeName":"后续审核1","nodeKey":"after1","type":1,
                 "childNode":{"nodeName":"后续审核2","nodeKey":"after2","type":1,"childNode":null}}}}
            """;
        var instance = CreateInstance("""{"type":"leader"}""");
        var approved = instance.CreateTask("leader", "领导审核", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("after1", [Assignee(3)]), ("after2", [Assignee(4)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Contains(instance.Tasks, t =>
            t.NodeKey == "after1"
            && t.TaskType == WorkflowTaskType.Approval
            && t.Status == WorkflowTaskStatus.Pending);
        Assert.DoesNotContain(instance.Tasks, t => t.NodeKey == "after2");
        Assert.Equal(WorkflowInstanceStatus.Running, instance.Status);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_CurrentNodeNotInResolvedConditionRoute_DoesNotCompleteEarly()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"条件路由","nodeKey":"route1","type":4,
               "conditionNodes":[
                 {"nodeName":"领导分支","nodeKey":"branch1","type":3,"priorityLevel":1,
                  "conditionList":[[{"field":"type","operator":"==","value":"leader"}]],
                  "childNode":{"nodeName":"领导审核","nodeKey":"leader","type":1,"childNode":null}},
                 {"nodeName":"其他情况","nodeKey":"branch2","type":3,"priorityLevel":2,
                  "conditionList":[],
                  "childNode":{"nodeName":"普通审核","nodeKey":"normal","type":1,"childNode":null}}
               ],
               "childNode":{"nodeName":"后续审核1","nodeKey":"after1","type":1,
                 "childNode":{"nodeName":"后续审核2","nodeKey":"after2","type":1,"childNode":null}}}}
            """;
        var instance = CreateInstance("""{"type":"normal"}""");
        var approved = instance.CreateTask("leader", "领导审核", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("after1", [Assignee(3)]), ("after2", [Assignee(4)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Contains(instance.Tasks, t =>
            t.NodeKey == "after1"
            && t.TaskType == WorkflowTaskType.Approval
            && t.Status == WorkflowTaskStatus.Pending);
        Assert.Equal(WorkflowInstanceStatus.Running, instance.Status);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_NextApproval_AllFilteredByDataPermission_ThrowsDataPermissionDenied()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,
               "childNode":{"nodeName":"审批2","nodeKey":"a2","type":1,"childNode":null}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = new WorkflowOutgoingTaskService(
            new FakeWorkflowApprovalAssignmentService(
                new Dictionary<string, IReadOnlyList<WorkflowAssigneeResult>>
                {
                    ["a2"] = [Assignee(20), Assignee(21)],
                },
                new UserAllowListVisibilityPolicy(/* 无允许用户，过滤后无人 */)),
            new WorkflowGraphRuntimeService());

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None));

        Assert.Equal(ErrorCodes.WorkflowAssigneeDataPermissionDenied, ex.ErrorCode);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_NextApproval_NoAssigneesResolved_ThrowsResolutionFailed()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,
               "childNode":{"nodeName":"审批2","nodeKey":"a2","type":1,"childNode":null}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = new WorkflowOutgoingTaskService(
            new FakeWorkflowApprovalAssignmentService(
                new Dictionary<string, IReadOnlyList<WorkflowAssigneeResult>>
                {
                    ["a2"] = [],
                },
                new PassthroughWorkflowTaskVisibilityPolicy()),
            new WorkflowGraphRuntimeService());

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None));

        Assert.Equal(ErrorCodes.WorkflowAssigneeResolutionFailed, ex.ErrorCode);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_NoNextNode_CompletesInstance()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,"childNode":null}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService();
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Equal(WorkflowInstanceStatus.Completed, instance.Status);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_TailCarbonCopyOnly_CompletesInstance()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,
               "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,"childNode":null}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "审批1", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("cc1", [Assignee(2)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.Equal(WorkflowInstanceStatus.Completed, instance.Status);
        var cc = Assert.Single(instance.Tasks, t => t.NodeKey == "cc1" && t.TaskType == WorkflowTaskType.CarbonCopy);
        Assert.Equal(WorkflowTaskStatus.Cancelled, cc.Status);
    }

    [Fact]
    public async Task AdvanceAfterTaskApprovedAsync_OfficeTaskParticipantNode_SkipsReceiverApprovalAndCompletes()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审核","nodeKey":"a1","type":1,
               "childNode":{"nodeName":"接收","nodeKey":"recv","type":1,"officeTaskParticipantNode":true,
                 "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,"childNode":null}}}}
            """;
        var instance = CreateInstance();
        var approved = instance.CreateTask("a1", "审核", WorkflowTaskType.Approval, new UserId(1), "用户1");
        instance.ApproveTask(approved.Id, new UserId(1), [], "同意");

        var service = CreateService(("recv", [Assignee(5)]), ("cc1", [Assignee(6)]));
        await service.AdvanceAfterTaskApprovedAsync(instance, approved.Id, CreateDefinition(json), CancellationToken.None);

        Assert.DoesNotContain(instance.Tasks, t => t.NodeKey == "recv");
        Assert.Equal(WorkflowInstanceStatus.Completed, instance.Status);
        Assert.Contains(instance.Tasks, t => t.NodeKey == "cc1" && t.TaskType == WorkflowTaskType.CarbonCopy);
    }

    private static WorkflowOutgoingTaskService CreateService(params (string NodeKey, IReadOnlyList<WorkflowAssigneeResult> Assignees)[] assignees)
    {
        return new WorkflowOutgoingTaskService(
            new FakeWorkflowApprovalAssignmentService(
                assignees.ToDictionary(x => x.NodeKey, x => x.Assignees),
                new PassthroughWorkflowTaskVisibilityPolicy()),
            new WorkflowGraphRuntimeService());
    }

    private static WorkflowDefinitionVersion CreateDefinition(string definitionJson)
    {
        var version = new WorkflowDefinitionVersion(DefinitionId, 1, definitionJson);
        version.Publish(WorkflowTestGraphHelper.ConvertLegacyTreeToGraphSnapshot(definitionJson), new UserId(1));
        return version;
    }

    private static WorkflowInstance CreateInstance(string variablesJson = "{}")
    {
        return new WorkflowInstance(
            DefinitionId,
            WorkflowDefinitionVersionId.Unassigned,
            "测试流程",
            Guid.NewGuid().ToString(),
            "Test",
            "测试流程",
            new UserId(99),
            "发起人",
            new DeptId(1),
            variablesJson,
            string.Empty);
    }

    private static WorkflowAssigneeResult Assignee(long userId)
    {
        return new WorkflowAssigneeResult(new UserId(userId), new RoleId(Guid.Empty), $"用户{userId}");
    }

    private sealed class FakeWorkflowApprovalAssignmentService(
        IReadOnlyDictionary<string, IReadOnlyList<WorkflowAssigneeResult>> assignees,
        IWorkflowTaskVisibilityPolicy visibility)
        : IWorkflowApprovalAssignmentService
    {
        public async Task<WorkflowAssigneeResolutionResult> ResolveForTaskCreationAsync(
            WorkflowGraphNode node,
            WorkflowInstance instance,
            string? definitionJson = null,
            CancellationToken cancellationToken = default)
        {
            var raw = assignees.GetValueOrDefault(node.NodeId) ?? ResolveApprovedAssigneesForCurrentNode(node, instance);
            if (node.Type != WorkflowGraphNodeType.Approval)
            {
                return new WorkflowAssigneeResolutionResult(raw, raw, [], false, []);
            }

            var filtered = await visibility.FilterAssigneesByDataPermissionAsync(instance, raw, cancellationToken);
            if (raw.Count > 0 && filtered.Count == 0)
            {
                var names = string.Join("、", raw.Select(x => x.DisplayName));
                throw new KnownException($"审批节点「{node.Name}」候选审批人均无数据权限：{names}", ErrorCodes.WorkflowAssigneeDataPermissionDenied);
            }

            if (filtered.Count == 0)
            {
                throw new KnownException($"审批节点「{node.Name}」未解析到有效审批人", ErrorCodes.WorkflowAssigneeResolutionFailed);
            }

            var filteredOut = raw.Where(r => filtered.All(f => f.AssigneeId != r.AssigneeId)).ToList();
            return new WorkflowAssigneeResolutionResult(filtered, raw, filteredOut, false, []);
        }

        private static IReadOnlyList<WorkflowAssigneeResult> ResolveApprovedAssigneesForCurrentNode(
            WorkflowGraphNode node,
            WorkflowInstance instance)
        {
            return instance.Tasks
                .Where(t =>
                    t.NodeKey == node.NodeId
                    && t.TaskType == WorkflowTaskType.Approval
                    && t.Status == WorkflowTaskStatus.Approved)
                .Select(t => new WorkflowAssigneeResult(t.AssigneeId, new RoleId(Guid.Empty), t.AssigneeName))
                .ToList();
        }
    }

    private sealed class PassthroughWorkflowTaskVisibilityPolicy : IWorkflowTaskVisibilityPolicy
    {
        public Task<IReadOnlyList<WorkflowAssigneeResult>> FilterAssigneesByDataPermissionAsync(
            WorkflowInstance instance,
            IReadOnlyList<WorkflowAssigneeResult> assignees,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(assignees);
        }

        public Task<bool> CanUserAccessWorkflowByDataPermissionAsync(
            UserId userId,
            UserId initiatorId,
            DeptId initiatorDeptId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

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
