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

