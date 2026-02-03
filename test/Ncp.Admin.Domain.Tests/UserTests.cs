using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.Tests;

/// <summary>
/// 用户聚合根（User）的领域逻辑单元测试。
/// 覆盖角色更新、年龄计算、角色名称同步、软删除、UserRole 值对象等业务规则。
/// </summary>
public class UserTests
{
    /// <summary>
    /// 使用新的角色集合更新用户角色时，应替换为新的角色集合（不同 RoleId）。
    /// </summary>
    [Fact]
    public void UpdateRoles_WithValidRoles_ShouldReplaceRoles()
    {
        var roleId1 = new RoleId(Guid.NewGuid());
        var roleId2 = new RoleId(Guid.NewGuid());
        var user = new User(
            "test",
            "13800000000",
            "pwd",
            [new UserRole(roleId1, "Admin")],
            "real",
            1,
            "email@test.com",
            "male",
            DateTimeOffset.UtcNow.AddYears(-20));

        var newRoles = new[] { new UserRole(roleId2, "User") };
        user.UpdateRoles(newRoles);

        Assert.Single(user.Roles);
        Assert.Equal(roleId2, user.Roles.First().RoleId);
        Assert.Equal("User", user.Roles.First().RoleName);
    }

    /// <summary>
    /// 使用空集合更新用户角色时，应清空所有角色。
    /// </summary>
    [Fact]
    public void UpdateRoles_WithEmptyRoles_ShouldClearAllRoles()
    {
        var roleId = new RoleId(Guid.NewGuid());
        var existingRoles = new[] { new UserRole(roleId, "Admin") };
        var user = new User(
            "test",
            "13800000000",
            "pwd",
            existingRoles,
            "real",
            1,
            "email@test.com",
            "male",
            DateTimeOffset.UtcNow.AddYears(-20));

        user.UpdateRoles(Enumerable.Empty<UserRole>());

        Assert.Empty(user.Roles);
    }

    /// <summary>
    /// 更新角色时若包含新增的 RoleId，应将该新角色加入集合。
    /// </summary>
    [Fact]
    public void UpdateRoles_AddNewRole_ShouldAddRole()
    {
        var roleId1 = new RoleId(Guid.NewGuid());
        var roleId2 = new RoleId(Guid.NewGuid());
        var user = new User(
            "test",
            "13800000000",
            "pwd",
            [new UserRole(roleId1, "Admin")],
            "real",
            1,
            "email@test.com",
            "male",
            DateTimeOffset.UtcNow.AddYears(-20));

        user.UpdateRoles(
        [
            new UserRole(roleId1, "Admin"),
            new UserRole(roleId2, "User")
        ]);

        Assert.Equal(2, user.Roles.Count);
        Assert.Contains(user.Roles, r => r.RoleId == roleId2 && r.RoleName == "User");
    }

    /// <summary>
    /// 根据出生日期计算年龄时，应返回正确的周岁（生日前一天仍为上一岁）。
    /// </summary>
    [Fact]
    public void CalculateAge_WithBirthDate_ShouldReturnCorrectAge()
    {
        var birthDate = DateTimeOffset.UtcNow.AddYears(-25).AddDays(-1);

        var age = User.CalculateAge(birthDate);

        Assert.Equal(25, age);
    }

    /// <summary>
    /// 生日为当天时，CalculateAge 应返回对应周岁。
    /// </summary>
    [Fact]
    public void CalculateAge_BirthdayToday_ShouldReturnCorrectAge()
    {
        var birthDate = DateTimeOffset.UtcNow.AddYears(-30);

        var age = User.CalculateAge(birthDate);

        Assert.Equal(30, age);
    }

    /// <summary>
    /// 更新已存在角色的名称时，应同步更新该 UserRole 的 RoleName。
    /// </summary>
    [Fact]
    public void UpdateRoleInfo_WithExistingRole_ShouldUpdateRoleName()
    {
        var roleId = new RoleId(Guid.NewGuid());
        var user = new User(
            "test",
            "13800000000",
            "pwd",
            [new UserRole(roleId, "Admin")],
            "real",
            1,
            "email@test.com",
            "male",
            DateTimeOffset.UtcNow.AddYears(-20));

        user.UpdateRoleInfo(roleId, "Administrator");

        Assert.Equal("Administrator", user.Roles.First().RoleName);
    }

    /// <summary>
    /// 软删除用户时，IsDeleted 应变为 true。
    /// </summary>
    [Fact]
    public void SoftDelete_ShouldSetIsDeleted()
    {
        var user = new User(
            "test",
            "13800000000",
            "pwd",
            [],
            "real",
            1,
            "email@test.com",
            "male",
            DateTimeOffset.UtcNow.AddYears(-20));

        user.SoftDelete();

        Assert.True(user.IsDeleted);
    }

    /// <summary>
    /// UserRole 值对象调用 UpdateRoleInfo 时，应更新 RoleName。
    /// </summary>
    [Fact]
    public void UserRole_UpdateRoleInfo_ShouldUpdateRoleName()
    {
        var roleId = new RoleId(Guid.NewGuid());
        var userRole = new UserRole(roleId, "OldName");

        userRole.UpdateRoleInfo("NewName");

        Assert.Equal("NewName", userRole.RoleName);
    }
}
