namespace Ncp.Admin.Web.AppPermissions
{
    /// <summary>
    /// 权限映射辅助类，用于通过权限代码自动获取权限名称和描述
    /// </summary>
    public static class PermissionMapper
    {
   
        /// <summary>
        /// 通过权限代码获取权限信息（名称和描述）
        /// </summary>
        /// <param name="permissionCode">权限代码</param>
        /// <returns>返回 (权限名称, 权限描述) 元组</returns>
        public static (string Name, string Description) GetPermissionInfo(string permissionCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);

            return PermissionDefinitionContext.PermissionsByCode.TryGetValue(permissionCode, out var permission)
                ? (permission.DisplayName, permission.Description)
                : (permissionCode, string.Empty);
        }
    }
}
