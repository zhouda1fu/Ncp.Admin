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

    #region 系统管理权限
    public const string SystemAdmin = nameof(SystemAdmin);
    public const string SystemMonitor = nameof(SystemMonitor);
    #endregion

    #region 生产计划管理权限
    public const string ProductionPlanManagement = nameof(ProductionPlanManagement);
    public const string ProductionPlanCreate = nameof(ProductionPlanCreate);
    public const string ProductionPlanEdit = nameof(ProductionPlanEdit);
    public const string ProductionPlanDelete = nameof(ProductionPlanDelete);
    public const string ProductionPlanView = nameof(ProductionPlanView);
    #endregion

    #region 工单管理权限
    public const string WorkOrderManagement = nameof(WorkOrderManagement);
    public const string WorkOrderCreate = nameof(WorkOrderCreate);
    public const string WorkOrderEdit = nameof(WorkOrderEdit);
    public const string WorkOrderDelete = nameof(WorkOrderDelete);
    public const string WorkOrderView = nameof(WorkOrderView);
    #endregion

    #region 工艺路线管理权限
    public const string RoutingManagement = nameof(RoutingManagement);
    public const string RoutingCreate = nameof(RoutingCreate);
    public const string RoutingEdit = nameof(RoutingEdit);
    public const string RoutingDelete = nameof(RoutingDelete);
    public const string RoutingView = nameof(RoutingView);
    #endregion

    #region 组织架构管理权限
    public const string OrganizationUnitManagement = nameof(OrganizationUnitManagement);
    public const string OrganizationUnitCreate = nameof(OrganizationUnitCreate);
    public const string OrganizationUnitEdit = nameof(OrganizationUnitEdit);
    public const string OrganizationUnitDelete = nameof(OrganizationUnitDelete);
    public const string OrganizationUnitView = nameof(OrganizationUnitView);
    public const string OrganizationUnitAssign = nameof(OrganizationUnitAssign);
    #endregion

    #region 所有接口访问权限
    public const string AllApiAccess = nameof(AllApiAccess);
    #endregion
}

