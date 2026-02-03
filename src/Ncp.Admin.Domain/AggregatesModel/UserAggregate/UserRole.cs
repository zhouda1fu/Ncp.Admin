using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// 用户角色值对象
/// 表示用户与角色的多对多关系中的角色信息，RoleName 冗余存储以避免关联查询。
/// </summary>
public class UserRole
{
    protected UserRole() { }

    public UserId UserId { get; private set; } = default!;
    public RoleId RoleId { get; private set; } = default!;
    public string RoleName { get; private set; } = string.Empty;

    /// <summary>
    /// 创建用户角色关联
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <param name="roleName">角色名称（冗余存储以避免关联查询）</param>
    public UserRole(RoleId roleId, string roleName)
    {
        RoleId = roleId;
        RoleName = roleName;
    }

    /// <summary>
    /// 更新角色名称（当角色信息变更时同步更新）
    /// </summary>
    /// <param name="roleName">新的角色名称</param>
    public void UpdateRoleInfo(string roleName)
    {
        RoleName = roleName;
    }
}

