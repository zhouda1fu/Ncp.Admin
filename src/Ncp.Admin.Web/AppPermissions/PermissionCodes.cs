namespace Ncp.Admin.Web.AppPermissions;

/// <summary>
/// 权限常量定义
/// </summary>
public static class PermissionCodes
{
    #region 角色管理权限
    public const string RoleManagement = nameof(RoleManagement);
    public const string RoleCreate = nameof(RoleCreate);
    public const string RoleEdit = nameof(RoleEdit);
    public const string RoleDelete = nameof(RoleDelete);
    public const string RoleView = nameof(RoleView);
    public const string RoleUpdatePermissions = nameof(RoleUpdatePermissions);
    #endregion

    #region 用户管理权限
    public const string UserManagement = nameof(UserManagement);
    public const string UserCreate = nameof(UserCreate);
    public const string UserEdit = nameof(UserEdit);
    public const string UserDelete = nameof(UserDelete);
    public const string UserView = nameof(UserView);
    public const string UserRoleAssign = nameof(UserRoleAssign);
    public const string UserResetPassword = nameof(UserResetPassword);
    #endregion

    #region 部门管理权限
    public const string DeptManagement = nameof(DeptManagement);
    public const string DeptCreate = nameof(DeptCreate);
    public const string DeptEdit = nameof(DeptEdit);
    public const string DeptDelete = nameof(DeptDelete);
    public const string DeptView = nameof(DeptView);
    #endregion

    #region 工作流管理权限
    public const string WorkflowManagement = nameof(WorkflowManagement);
    public const string WorkflowDefinitionView = nameof(WorkflowDefinitionView);
    public const string WorkflowDefinitionCreate = nameof(WorkflowDefinitionCreate);
    public const string WorkflowDefinitionEdit = nameof(WorkflowDefinitionEdit);
    public const string WorkflowDefinitionDelete = nameof(WorkflowDefinitionDelete);
    public const string WorkflowDefinitionPublish = nameof(WorkflowDefinitionPublish);
    public const string WorkflowStart = nameof(WorkflowStart);
    public const string WorkflowCancel = nameof(WorkflowCancel);
    public const string WorkflowTaskApprove = nameof(WorkflowTaskApprove);
    public const string WorkflowInstanceView = nameof(WorkflowInstanceView);
    public const string WorkflowMonitor = nameof(WorkflowMonitor);
    #endregion

    #region 所有接口访问权限
    public const string AllApiAccess = nameof(AllApiAccess);
    #endregion
}

