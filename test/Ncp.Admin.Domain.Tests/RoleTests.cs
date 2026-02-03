using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.Admin.Domain.Tests;

/// <summary>
/// 角色聚合根（Role）的领域逻辑单元测试。
/// 覆盖角色信息更新、权限更新、激活/停用、软删除、权限判断等业务规则。
/// </summary>
public class RoleTests
{
    /// <summary>
    /// 创建角色时，应正确设置名称、描述和权限集合，且默认处于激活状态。
    /// </summary>
    [Fact]
    public void Constructor_WithValidArgs_ShouldSetProperties()
    {
        var permissions = new[] { new RolePermission("user:read", "查看用户", "查看用户列表") };
        var role = new Role("管理员", "系统管理员", permissions);

        Assert.Equal("管理员", role.Name);
        Assert.Equal("系统管理员", role.Description);
        Assert.Single(role.Permissions);
        Assert.True(role.IsActive);
        Assert.False(role.IsDeleted);
    }

    /// <summary>
    /// 更新角色信息时，应更新名称和描述。
    /// </summary>
    [Fact]
    public void UpdateRoleInfo_WithValidArgs_ShouldUpdateNameAndDescription()
    {
        var role = new Role("管理员", "系统管理员", []);

        role.UpdateRoleInfo("超级管理员", "拥有全部权限");

        Assert.Equal("超级管理员", role.Name);
        Assert.Equal("拥有全部权限", role.Description);
    }

    /// <summary>
    /// 更新角色权限时，应移除不再包含的权限并添加新权限，保持与目标集合一致。
    /// </summary>
    [Fact]
    public void UpdateRolePermissions_WithNewSet_ShouldReplacePermissions()
    {
        var role = new Role("管理员", "描述", [
            new RolePermission("user:read", "查看用户", "描述"),
            new RolePermission("user:write", "编辑用户", "描述")
        ]);
        var newPermissions = new[]
        {
            new RolePermission("user:read", "查看用户", "描述"),
            new RolePermission("dept:read", "查看部门", "描述")
        };

        role.UpdateRolePermissions(newPermissions);

        Assert.Equal(2, role.Permissions.Count);
        Assert.True(role.HasPermission("user:read"));
        Assert.True(role.HasPermission("dept:read"));
        Assert.False(role.HasPermission("user:write"));
    }

    /// <summary>
    /// 更新角色权限为空集合时，应清空所有权限。
    /// </summary>
    [Fact]
    public void UpdateRolePermissions_WithEmpty_ShouldClearAllPermissions()
    {
        var role = new Role("管理员", "描述", [
            new RolePermission("user:read", "查看用户", "描述")
        ]);

        role.UpdateRolePermissions([]);

        Assert.Empty(role.Permissions);
        Assert.False(role.HasPermission("user:read"));
    }

    /// <summary>
    /// 停用已激活的角色时，IsActive 应变为 false。
    /// </summary>
    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalse()
    {
        var role = new Role("管理员", "描述", []);

        role.Deactivate();

        Assert.False(role.IsActive);
    }

    /// <summary>
    /// 停用已经停用的角色时，应抛出已知异常。
    /// </summary>
    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_ShouldThrow()
    {
        var role = new Role("管理员", "描述", []);
        role.Deactivate();

        var ex = Assert.Throws<KnownException>(() => role.Deactivate());

        Assert.NotNull(ex.Message);
        Assert.Contains("已经", ex.Message);
    }

    /// <summary>
    /// 激活已停用的角色时，IsActive 应变为 true。
    /// </summary>
    [Fact]
    public void Activate_WhenDeactivated_ShouldSetIsActiveToTrue()
    {
        var role = new Role("管理员", "描述", []);
        role.Deactivate();

        role.Activate();

        Assert.True(role.IsActive);
    }

    /// <summary>
    /// 激活已经激活的角色时，应抛出已知异常。
    /// </summary>
    [Fact]
    public void Activate_WhenAlreadyActivated_ShouldThrow()
    {
        var role = new Role("管理员", "描述", []);

        var ex = Assert.Throws<KnownException>(() => role.Activate());

        Assert.NotNull(ex.Message);
        Assert.Contains("已经", ex.Message);
    }

    /// <summary>
    /// 软删除角色时，IsDeleted 应变为 true。
    /// </summary>
    [Fact]
    public void SoftDelete_ShouldSetIsDeleted()
    {
        var role = new Role("管理员", "描述", []);

        role.SoftDelete();

        Assert.True(role.IsDeleted);
    }

    /// <summary>
    /// 当角色包含指定权限码时，HasPermission 应返回 true。
    /// </summary>
    [Fact]
    public void HasPermission_WhenPermissionExists_ShouldReturnTrue()
    {
        var role = new Role("管理员", "描述", [
            new RolePermission("user:read", "查看用户", "描述")
        ]);

        Assert.True(role.HasPermission("user:read"));
    }

    /// <summary>
    /// 当角色不包含指定权限码时，HasPermission 应返回 false。
    /// </summary>
    [Fact]
    public void HasPermission_WhenPermissionNotExists_ShouldReturnFalse()
    {
        var role = new Role("管理员", "描述", [
            new RolePermission("user:read", "查看用户", "描述")
        ]);

        Assert.False(role.HasPermission("user:write"));
    }

    /// <summary>
    /// 当角色无任何权限时，HasPermission 应返回 false。
    /// </summary>
    [Fact]
    public void HasPermission_WhenNoPermissions_ShouldReturnFalse()
    {
        var role = new Role("访客", "描述", []);

        Assert.False(role.HasPermission("user:read"));
    }

    /// <summary>
    /// RolePermission 更新权限信息时，应更新 PermissionName 和 PermissionDescription。
    /// </summary>
    [Fact]
    public void RolePermission_UpdatePermissionInfo_ShouldUpdateNameAndDescription()
    {
        var perm = new RolePermission("code", "旧名称", "旧描述");

        perm.UpdatePermissionInfo("新名称", "新描述");

        Assert.Equal("新名称", perm.PermissionName);
        Assert.Equal("新描述", perm.PermissionDescription);
    }
}
