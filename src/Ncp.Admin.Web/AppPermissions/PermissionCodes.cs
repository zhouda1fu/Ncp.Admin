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

    #region 岗位管理权限
    public const string PositionManagement = nameof(PositionManagement);
    public const string PositionCreate = nameof(PositionCreate);
    public const string PositionEdit = nameof(PositionEdit);
    public const string PositionDelete = nameof(PositionDelete);
    public const string PositionView = nameof(PositionView);
    #endregion

    #region 通知管理权限
    public const string NotificationManagement = nameof(NotificationManagement);
    public const string NotificationView = nameof(NotificationView);
    public const string NotificationSend = nameof(NotificationSend);
    #endregion

    #region 请假管理权限
    public const string LeaveManagement = nameof(LeaveManagement);
    public const string LeaveRequestView = nameof(LeaveRequestView);
    public const string LeaveRequestCreate = nameof(LeaveRequestCreate);
    public const string LeaveRequestEdit = nameof(LeaveRequestEdit);
    public const string LeaveRequestSubmit = nameof(LeaveRequestSubmit);
    public const string LeaveRequestCancel = nameof(LeaveRequestCancel);
    public const string LeaveBalanceView = nameof(LeaveBalanceView);
    public const string LeaveBalanceEdit = nameof(LeaveBalanceEdit);
    #endregion

    #region 所有接口访问权限
    public const string AllApiAccess = nameof(AllApiAccess);
    #endregion
}

