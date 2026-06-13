namespace Ncp.Admin.Web.AppPermissions;

/// <summary>
/// 平台脚手架权限常量（IAM + 工作流 + 平台基础设施）。
/// </summary>
public static class PermissionCodes
{
    public const string AllApiAccess = nameof(AllApiAccess);

    #region 模块入口
    public const string SystemModule = nameof(SystemModule);
    #endregion

    #region 角色
    public const string RoleManagement = nameof(RoleManagement);
    public const string RoleCreate = nameof(RoleCreate);
    public const string RoleEdit = nameof(RoleEdit);
    public const string RoleDelete = nameof(RoleDelete);
    public const string RoleView = nameof(RoleView);
    public const string RoleUpdatePermissions = nameof(RoleUpdatePermissions);
    #endregion

    #region 用户
    public const string UserManagement = nameof(UserManagement);
    public const string UserCreate = nameof(UserCreate);
    public const string UserEdit = nameof(UserEdit);
    public const string UserDelete = nameof(UserDelete);
    public const string UserView = nameof(UserView);
    public const string UserRoleAssign = nameof(UserRoleAssign);
    public const string UserResetPassword = nameof(UserResetPassword);
    public const string UserExport = nameof(UserExport);
    public const string UserImport = nameof(UserImport);
    public const string UserChangeHistoryView = nameof(UserChangeHistoryView);
    #endregion

    #region 部门
    public const string DeptManagement = nameof(DeptManagement);
    public const string DeptCreate = nameof(DeptCreate);
    public const string DeptEdit = nameof(DeptEdit);
    public const string DeptDelete = nameof(DeptDelete);
    public const string DeptView = nameof(DeptView);
    #endregion

    #region 岗位
    public const string PositionManagement = nameof(PositionManagement);
    public const string PositionCreate = nameof(PositionCreate);
    public const string PositionEdit = nameof(PositionEdit);
    public const string PositionDelete = nameof(PositionDelete);
    public const string PositionView = nameof(PositionView);
    #endregion

    #region 工作流
    public const string WorkflowManagement = nameof(WorkflowManagement);
    public const string WorkflowDefinitionView = nameof(WorkflowDefinitionView);
    public const string WorkflowDefinitionCreate = nameof(WorkflowDefinitionCreate);
    public const string WorkflowDefinitionEdit = nameof(WorkflowDefinitionEdit);
    public const string WorkflowDefinitionDelete = nameof(WorkflowDefinitionDelete);
    public const string WorkflowDefinitionDeletePublished = nameof(WorkflowDefinitionDeletePublished);
    public const string WorkflowDefinitionPublish = nameof(WorkflowDefinitionPublish);
    public const string WorkflowStart = nameof(WorkflowStart);
    public const string WorkflowCancel = nameof(WorkflowCancel);
    public const string WorkflowTaskApprove = nameof(WorkflowTaskApprove);
    public const string WorkflowInstanceView = nameof(WorkflowInstanceView);
    public const string WorkflowMonitor = nameof(WorkflowMonitor);
    #endregion

    #region 通知
    public const string NotificationManagement = nameof(NotificationManagement);
    public const string NotificationView = nameof(NotificationView);
    public const string NotificationSend = nameof(NotificationSend);
    #endregion

    #region 操作日志
    public const string OperationLogManagement = nameof(OperationLogManagement);
    public const string OperationLogView = nameof(OperationLogView);
    #endregion

    #region 系统日志
    public const string SystemLogManagement = nameof(SystemLogManagement);
    public const string SystemLogView = nameof(SystemLogView);
    #endregion

    #region 后台任务
    public const string BackgroundJobManagement = nameof(BackgroundJobManagement);
    public const string BackgroundJobView = nameof(BackgroundJobView);
    public const string BackgroundJobTrigger = nameof(BackgroundJobTrigger);
    #endregion

    #region 首页
    public const string HomeDashboard = nameof(HomeDashboard);
    #endregion
}
