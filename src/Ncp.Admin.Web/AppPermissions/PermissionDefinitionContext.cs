using System.Collections.Immutable;

namespace Ncp.Admin.Web.AppPermissions;

/// <summary>
/// 平台脚手架权限定义。
/// </summary>
public static class PermissionDefinitionContext
{
    private static Dictionary<string, AppPermissionGroup> Groups { get; } = new();

    private static IReadOnlyDictionary<string, AppPermission>? _permissionsByCode;

    static PermissionDefinitionContext()
    {
        var systemAccess = AddGroup("SystemAccess");
        var systemModule = systemAccess.AddPermission(PermissionCodes.SystemModule, "系统模块", "系统管理菜单入口");

        var userManagement = systemModule.AddChild(PermissionCodes.UserManagement, "用户管理");
        userManagement.AddChild(PermissionCodes.UserCreate, "创建用户");
        userManagement.AddChild(PermissionCodes.UserEdit, "编辑用户");
        userManagement.AddChild(PermissionCodes.UserDelete, "删除用户");
        userManagement.AddChild(PermissionCodes.UserView, "查看用户");
        userManagement.AddChild(PermissionCodes.UserRoleAssign, "分配用户角色");
        userManagement.AddChild(PermissionCodes.UserResetPassword, "重置用户密码");
        userManagement.AddChild(PermissionCodes.UserExport, "导出用户");
        userManagement.AddChild(PermissionCodes.UserImport, "导入用户");
        userManagement.AddChild(PermissionCodes.UserChangeHistoryView, "用户修改记录");

        var roleManagement = systemModule.AddChild(PermissionCodes.RoleManagement, "角色管理");
        roleManagement.AddChild(PermissionCodes.RoleCreate, "创建角色");
        roleManagement.AddChild(PermissionCodes.RoleEdit, "编辑角色");
        roleManagement.AddChild(PermissionCodes.RoleDelete, "删除角色");
        roleManagement.AddChild(PermissionCodes.RoleView, "查看角色");
        roleManagement.AddChild(PermissionCodes.RoleUpdatePermissions, "更新角色权限");

        var deptManagement = systemModule.AddChild(PermissionCodes.DeptManagement, "部门管理");
        deptManagement.AddChild(PermissionCodes.DeptCreate, "创建部门");
        deptManagement.AddChild(PermissionCodes.DeptEdit, "编辑部门");
        deptManagement.AddChild(PermissionCodes.DeptDelete, "删除部门");
        deptManagement.AddChild(PermissionCodes.DeptView, "查看部门");

        var positionManagement = systemModule.AddChild(PermissionCodes.PositionManagement, "岗位管理");
        positionManagement.AddChild(PermissionCodes.PositionCreate, "创建岗位");
        positionManagement.AddChild(PermissionCodes.PositionEdit, "编辑岗位");
        positionManagement.AddChild(PermissionCodes.PositionDelete, "删除岗位");
        positionManagement.AddChild(PermissionCodes.PositionView, "查看岗位");

        var workflowManagement = systemModule.AddChild(PermissionCodes.WorkflowManagement, "工作流管理");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionView, "查看流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionCreate, "创建流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionEdit, "编辑流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionDelete, "删除流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionDeletePublished, "删除已发布流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionPublish, "发布流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowStart, "发起流程");
        workflowManagement.AddChild(PermissionCodes.WorkflowCancel, "撤销流程");
        workflowManagement.AddChild(PermissionCodes.WorkflowTaskApprove, "审批任务");
        workflowManagement.AddChild(PermissionCodes.WorkflowInstanceView, "查看流程实例");
        workflowManagement.AddChild(PermissionCodes.WorkflowMonitor, "流程监控");

        var notificationManagement = systemModule.AddChild(PermissionCodes.NotificationManagement, "通知管理");
        notificationManagement.AddChild(PermissionCodes.NotificationView, "查看通知");
        notificationManagement.AddChild(PermissionCodes.NotificationSend, "发送通知");

        var operationLogManagement = systemModule.AddChild(PermissionCodes.OperationLogManagement, "操作日志");
        operationLogManagement.AddChild(PermissionCodes.OperationLogView, "查看操作日志");

        var systemLogManagement = systemModule.AddChild(PermissionCodes.SystemLogManagement, "系统日志");
        systemLogManagement.AddChild(PermissionCodes.SystemLogView, "查看系统日志");

        var backgroundJobManagement = systemModule.AddChild(PermissionCodes.BackgroundJobManagement, "后台任务");
        backgroundJobManagement.AddChild(PermissionCodes.BackgroundJobView, "查看后台任务");
        backgroundJobManagement.AddChild(PermissionCodes.BackgroundJobTrigger, "触发后台任务");

        systemModule.AddChild(PermissionCodes.HomeDashboard, "首页工作台");
    }

    public static IReadOnlyDictionary<string, AppPermission> PermissionsByCode =>
        _permissionsByCode ??= BuildPermissionsByCode();

    public static IReadOnlyList<AppPermissionGroup> GroupsList => Groups.Values.ToList();

    private static AppPermissionGroup AddGroup(string name)
    {
        var group = new AppPermissionGroup(name);
        Groups[name] = group;
        return group;
    }

    private static IReadOnlyDictionary<string, AppPermission> BuildPermissionsByCode()
    {
        var dict = new Dictionary<string, AppPermission>(StringComparer.Ordinal);
        foreach (var group in Groups.Values)
        {
            foreach (var permission in group.Permissions)
            {
                CollectPermissions(permission, dict);
            }
        }

        return dict.ToImmutableDictionary(StringComparer.Ordinal);
    }

    private static void CollectPermissions(AppPermission permission, Dictionary<string, AppPermission> dict)
    {
        dict[permission.Code] = permission;
        foreach (var child in permission.Children)
        {
            CollectPermissions(child, dict);
        }
    }
}
