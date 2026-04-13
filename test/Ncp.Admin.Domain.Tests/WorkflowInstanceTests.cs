using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Domain.Tests;

/// <summary>
/// 工作流实例聚合根单元测试：状态流转、会签/或签判定。
/// </summary>
public class WorkflowInstanceTests
{
    private static readonly WorkflowDefinitionId DefId = new(Guid.NewGuid());
    private static readonly UserId InitiatorId = new(1);
    private static readonly DeptId InitiatorDeptId = new(1);
    private static readonly UserId AssigneeId = new(2);
    private static readonly RoleId TestRoleId = new(Guid.NewGuid());

    private static WorkflowInstance CreateRunningInstance()
    {
        return new WorkflowInstance(
            DefId,
            "测试流程",
            "biz-001",
            "LeaveRequest",
            "请假申请",
            InitiatorId,
            "张三",
            InitiatorDeptId,
            "{}",
            "");
    }

    [Fact]
    public void Constructor_WithValidArgs_ShouldSetStatusRunning()
    {
        var instance = CreateRunningInstance();

        Assert.Equal(WorkflowInstanceStatus.Running, instance.Status);
        Assert.Equal("biz-001", instance.BusinessKey);
        Assert.Equal(InitiatorId, instance.InitiatorId);
    }

    [Fact]
    public void CreateTask_WithUser_ShouldAddTaskAndSetCurrentNode()
    {
        var instance = CreateRunningInstance();

        var task = instance.CreateTask("node1", "部门审批", WorkflowTaskType.Approval, AssigneeId, "李四");

        Assert.Single(instance.Tasks);
        Assert.Equal("node1", instance.CurrentNodeKey);
        Assert.Equal(WorkflowTaskStatus.Pending, task.Status);
    }

    [Fact]
    public void CreateTaskForRole_ShouldAddTask()
    {
        var instance = CreateRunningInstance();

        instance.CreateTaskForRole("node1", "角色审批", WorkflowTaskType.Approval, TestRoleId, "财务");

        Assert.Single(instance.Tasks);
        Assert.Equal(AssigneeType.Role, instance.Tasks.First().AssigneeType);
    }

    [Fact]
    public void ApproveTask_WhenRoleAssignedAndOperatorInRole_ShouldApprove()
    {
        var instance = CreateRunningInstance();
        var task = instance.CreateTaskForRole("node1", "角色审批", WorkflowTaskType.Approval, TestRoleId, "财务");

        instance.ApproveTask(task.Id, new UserId(999), new[] { TestRoleId }, "同意");

        Assert.Equal(WorkflowTaskStatus.Approved, task.Status);
    }

    [Fact]
    public void ApproveTask_WhenRoleAssignedAndOperatorNotInRole_ShouldThrow()
    {
        var instance = CreateRunningInstance();
        var task = instance.CreateTaskForRole("node1", "角色审批", WorkflowTaskType.Approval, TestRoleId, "财务");

        var ex = Assert.Throws<KnownException>(() =>
            instance.ApproveTask(task.Id, new UserId(999), new[] { new RoleId(Guid.NewGuid()) }, "同意"));

        Assert.Equal(ErrorCodes.WorkflowTaskNotAssignedToOperator, ex.ErrorCode);
    }

    [Fact]
    public void ApproveTask_WhenPending_ShouldMarkTaskApproved()
    {
        var instance = CreateRunningInstance();
        var task = instance.CreateTask("node1", "审批", WorkflowTaskType.Approval, AssigneeId, "李四");

        instance.ApproveTask(task.Id, AssigneeId, operatorRoleIds: null, "同意");

        Assert.Equal(WorkflowTaskStatus.Approved, task.Status);
        Assert.Equal("同意", task.Comment);
    }

    [Fact]
    public void ApproveTask_WhenTaskNotFound_ShouldThrow()
    {
        var instance = CreateRunningInstance();
        var unknownId = new WorkflowTaskId(Guid.NewGuid());

        var ex = Assert.Throws<KnownException>(() => instance.ApproveTask(unknownId, AssigneeId, operatorRoleIds: null, "同意"));

        Assert.Equal(ErrorCodes.WorkflowTaskNotFound, ex.ErrorCode);
    }

    [Fact]
    public void RejectTask_ShouldSetInstanceRejected()
    {
        var instance = CreateRunningInstance();
        var task = instance.CreateTask("node1", "审批", WorkflowTaskType.Approval, AssigneeId, "李四");

        instance.RejectTask(task.Id, AssigneeId, operatorRoleIds: null, "不同意");

        Assert.Equal(WorkflowInstanceStatus.Rejected, instance.Status);
        Assert.NotNull(instance.CompletedAt);
        Assert.Equal(WorkflowTaskStatus.Rejected, task.Status);
    }

    [Fact]
    public void RejectTask_ShouldCancelOtherPendingTasks()
    {
        var instance = CreateRunningInstance();
        var t1 = instance.CreateTask("node1", "或签A", WorkflowTaskType.Approval, AssigneeId, "A");
        var t2 = instance.CreateTask("node1", "或签B", WorkflowTaskType.Approval, new UserId(3), "B");

        instance.RejectTask(t1.Id, AssigneeId, operatorRoleIds: null, "不同意");

        Assert.Equal(WorkflowTaskStatus.Rejected, t1.Status);
        Assert.Equal(WorkflowTaskStatus.Cancelled, t2.Status);
    }

    [Fact]
    public void Complete_ShouldCancelPendingCarbonCopyTasks()
    {
        var instance = CreateRunningInstance();
        instance.CreateTask("n1", "审批", WorkflowTaskType.Approval, AssigneeId, "审");
        var cc = instance.CreateTask("n2", "抄送", WorkflowTaskType.CarbonCopy, new UserId(4), "抄");

        instance.Complete();

        Assert.Equal(WorkflowInstanceStatus.Completed, instance.Status);
        Assert.Equal(WorkflowTaskStatus.Cancelled, cc.Status);
    }

    [Fact]
    public void AreAllCounterSignTasksApproved_WhenSingleTaskApproved_ShouldReturnTrue()
    {
        var instance = CreateRunningInstance();
        var task = instance.CreateTask("node1", "会签", WorkflowTaskType.Approval, AssigneeId, "A");
        instance.ApproveTask(task.Id, AssigneeId, operatorRoleIds: null, "同意");

        Assert.True(instance.AreAllCounterSignTasksApproved("node1"));
    }

    [Fact]
    public void AreAllCounterSignTasksApproved_WhenOnePending_ShouldReturnFalse()
    {
        var instance = CreateRunningInstance();
        instance.CreateTask("node1", "会签", WorkflowTaskType.Approval, AssigneeId, "A");
        instance.CreateTask("node1", "会签", WorkflowTaskType.Approval, new UserId(3), "B");
        var firstTask = instance.Tasks.First(t => t.AssigneeId == AssigneeId);
        instance.ApproveTask(firstTask.Id, AssigneeId, operatorRoleIds: null, "同意");

        Assert.False(instance.AreAllCounterSignTasksApproved("node1"));
    }

    [Fact]
    public void IsAnyOrSignTaskApproved_WhenOneApproved_ShouldReturnTrue()
    {
        var instance = CreateRunningInstance();
        instance.CreateTask("node1", "或签", WorkflowTaskType.Approval, AssigneeId, "A");
        var task = instance.Tasks.Single();
        instance.ApproveTask(task.Id, AssigneeId, operatorRoleIds: null, "同意");

        Assert.True(instance.IsAnyOrSignTaskApproved("node1"));
    }

    [Fact]
    public void Complete_WhenRunning_ShouldSetCompleted()
    {
        var instance = CreateRunningInstance();

        instance.Complete();

        Assert.Equal(WorkflowInstanceStatus.Completed, instance.Status);
        Assert.NotNull(instance.CompletedAt);
    }

    [Fact]
    public void Complete_WhenNotRunning_ShouldThrow()
    {
        var instance = CreateRunningInstance();
        instance.Complete();

        var ex = Assert.Throws<KnownException>(() => instance.Complete());

        Assert.Equal(ErrorCodes.WorkflowInstanceNotRunning, ex.ErrorCode);
    }

    [Fact]
    public void Cancel_ByInitiator_ShouldSetCancelled()
    {
        var instance = CreateRunningInstance();

        instance.Cancel(InitiatorId);

        Assert.Equal(WorkflowInstanceStatus.Cancelled, instance.Status);
    }

    [Fact]
    public void Cancel_ByNonInitiator_ShouldThrow()
    {
        var instance = CreateRunningInstance();

        var ex = Assert.Throws<KnownException>(() => instance.Cancel(new UserId(999)));

        Assert.Equal(ErrorCodes.WorkflowOnlyInitiatorCanCancel, ex.ErrorCode);
    }
}
