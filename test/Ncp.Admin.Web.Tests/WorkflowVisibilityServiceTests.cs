using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Tests;

public class WorkflowVisibilityServiceTests
{
    private static readonly WorkflowDefinitionId DefinitionId = new(Guid.Parse("44444444-4444-4444-4444-444444444444"));
    private static readonly RoleId ApprovalRoleId = new(Guid.Parse("55555555-5555-5555-5555-555555555555"));
    private static readonly RoleId OtherRoleId = new(Guid.Parse("66666666-6666-6666-6666-666666666666"));

    [Fact]
    public void ApplyTaskDisplayFilter_UserSnapshot_IgnoresInitiatorDeptScope()
    {
        var service = new WorkflowVisibilityService(null!);
        var currentUserId = new UserId(10);
        var dataPermission = new DataPermissionContext(DataScope.Dept, currentUserId, new DeptId(1), []);
        var visible = CreateProjection(
            initiatorDeptId: new DeptId(99),
            WorkflowTaskAssignmentSnapshot.ForUser(
                currentUserId,
                "审批人",
                WorkflowAssignmentSource.Member,
                "member",
                WorkflowTaskVisibilityMode.ExplicitUser,
                false,
                WorkflowTaskInitiatorDeptScopeMode.DataPermission,
                "[]",
                "test"));

        var result = service.ApplyTaskDisplayFilter(
                new[] { visible }.AsQueryable(),
                dataPermission,
                currentUserId,
                [])
            .ToList();

        Assert.Single(result);
    }

    [Fact]
    public void ApplyTaskDisplayFilter_RoleSnapshot_RequiresDataPermissionUnlessBypassed()
    {
        var service = new WorkflowVisibilityService(null!);
        var currentUserId = new UserId(10);
        var roleId = ApprovalRoleId;
        var dataPermission = new DataPermissionContext(DataScope.Dept, currentUserId, new DeptId(1), []);
        var hidden = CreateProjection(
            initiatorDeptId: new DeptId(99),
            WorkflowTaskAssignmentSnapshot.ForRole(
                roleId,
                "审批角色",
                WorkflowAssignmentSource.Role,
                "role",
                WorkflowTaskVisibilityMode.RoleDataPermission,
                false,
                WorkflowTaskInitiatorDeptScopeMode.DataPermission,
                "[]",
                "test"));
        var visible = CreateProjection(
            initiatorDeptId: new DeptId(99),
            WorkflowTaskAssignmentSnapshot.ForRole(
                roleId,
                "跨部门审批角色",
                WorkflowAssignmentSource.Role,
                "role-bypass",
                WorkflowTaskVisibilityMode.BypassDataPermission,
                true,
                WorkflowTaskInitiatorDeptScopeMode.All,
                "[]",
                "test"));

        var result = service.ApplyTaskDisplayFilter(
                new[] { hidden, visible }.AsQueryable(),
                dataPermission,
                currentUserId,
                [roleId])
            .ToList();

        var item = Assert.Single(result);
        Assert.True(item.Snapshot.BypassDataPermission);
    }

    [Fact]
    public void CanOperateTask_PendingTaskRequiresSnapshotMatch()
    {
        var service = new WorkflowVisibilityService(null!);
        var instance = CreateInstance(new DeptId(1));
        var task = instance.CreateTask("a1", "审批", WorkflowTaskType.Approval, UserId.Unassigned, "审批角色");
        var roleId = ApprovalRoleId;
        task.AssignmentSnapshots.Add(WorkflowTaskAssignmentSnapshot.ForRole(
            roleId,
            "审批角色",
            WorkflowAssignmentSource.Role,
            "role",
            WorkflowTaskVisibilityMode.RoleDataPermission,
            false,
            WorkflowTaskInitiatorDeptScopeMode.DataPermission,
            "[]",
            "test"));
        var snapshots = new Dictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>>();

        Assert.True(service.CanOperateTask(task, snapshots, new UserId(10), [roleId]));
        Assert.False(service.CanOperateTask(task, snapshots, new UserId(10), [OtherRoleId]));
    }

    private static WorkflowTaskSnapshotProjection CreateProjection(
        DeptId initiatorDeptId,
        WorkflowTaskAssignmentSnapshot snapshot)
    {
        var instance = CreateInstance(initiatorDeptId);
        var task = instance.CreateTask("a1", "审批", WorkflowTaskType.Approval, UserId.Unassigned, "审批人");
        return new WorkflowTaskSnapshotProjection
        {
            Instance = instance,
            Task = task,
            Snapshot = snapshot
        };
    }

    private static WorkflowInstance CreateInstance(DeptId initiatorDeptId)
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
            initiatorDeptId,
            "{}",
            string.Empty);
    }
}
