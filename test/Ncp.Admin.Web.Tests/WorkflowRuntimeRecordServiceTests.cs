using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Tests;

public class WorkflowRuntimeRecordServiceTests
{
    private static readonly WorkflowDefinitionId DefinitionId = new(Guid.Parse("33333333-3333-3333-3333-333333333333"));

    [Fact]
    public async Task RecordTaskCreatedAsync_UsesAssigneeMetadata_WhenCreatingSnapshot()
    {
        var instance = CreateInstance();
        var task = instance.CreateTask("a1", "审批", WorkflowTaskType.Approval, new UserId(10), "审批人");
        var assignee = new WorkflowAssigneeResult(
            new UserId(10),
            RoleId.Unassigned,
            "审批人",
            true,
            WorkflowAssignmentSource.Role,
            "rule-role-1",
            WorkflowTaskVisibilityMode.RoleDataPermissionPlusConfiguredDept,
            WorkflowTaskInitiatorDeptScopeMode.SpecifiedDeptAndSub,
            """["1","2"]""");

        await new WorkflowRuntimeRecordService().RecordTaskCreatedAsync(
            instance,
            [new WorkflowCreatedTask(task, assignee)],
            "start",
            CancellationToken.None);

        var snapshot = Assert.Single(task.AssignmentSnapshots);
        Assert.Equal(WorkflowAssignmentSource.Role, snapshot.AssignmentSource);
        Assert.Equal("rule-role-1", snapshot.SourceRuleId);
        Assert.Equal(WorkflowTaskVisibilityMode.RoleDataPermissionPlusConfiguredDept, snapshot.VisibilityMode);
        Assert.True(snapshot.BypassDataPermission);
        Assert.Equal(WorkflowTaskInitiatorDeptScopeMode.SpecifiedDeptAndSub, snapshot.InitiatorDeptScopeMode);
        Assert.Equal("""["1","2"]""", snapshot.InitiatorDeptScopeDeptIdsJson);
        Assert.Equal("start", snapshot.CreatedReason);
    }

    [Fact]
    public async Task RecordTaskCreatedAsync_UsesTransferredSource_ForTransferredTask()
    {
        var instance = CreateInstance();
        var task = instance.CreateTask("a1", "审批", WorkflowTaskType.Approval, new UserId(11), "新处理人");
        var assignee = new WorkflowAssigneeResult(
            new UserId(11),
            RoleId.Unassigned,
            "新处理人",
            true,
            WorkflowAssignmentSource.Transferred,
            "original-task-id",
            WorkflowTaskVisibilityMode.ExplicitUser,
            WorkflowTaskInitiatorDeptScopeMode.All,
            "[]");

        await new WorkflowRuntimeRecordService().RecordTaskCreatedAsync(
            instance,
            [new WorkflowCreatedTask(task, assignee)],
            "transfer",
            CancellationToken.None);

        var snapshot = Assert.Single(task.AssignmentSnapshots);
        Assert.Equal(WorkflowAssignmentSource.Transferred, snapshot.AssignmentSource);
        Assert.Equal("original-task-id", snapshot.SourceRuleId);
        Assert.Equal(WorkflowTaskVisibilityMode.ExplicitUser, snapshot.VisibilityMode);
        Assert.Equal(WorkflowTaskInitiatorDeptScopeMode.All, snapshot.InitiatorDeptScopeMode);
    }

    private static WorkflowInstance CreateInstance()
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
            "{}",
            string.Empty);
    }
}
