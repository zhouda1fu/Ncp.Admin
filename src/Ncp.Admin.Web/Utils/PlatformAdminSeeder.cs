using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Utils;

/// <summary>
/// 初始化默认管理员账号、根部门与超级管理员角色（仅当库中无用户时执行）。
/// </summary>
public static class PlatformAdminSeeder
{
    public const string DefaultAdminName = "admin";
    public const string DefaultAdminPassword = "Admin@123456";

    public static async Task EnsureSeededAsync(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var rootDept = new Dept("总公司", string.Empty, DeptId.Unassigned, 1, 0);
        dbContext.Depts.Add(rootDept);

        var permissionCodes = typeof(PermissionCodes)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Where(c => c != PermissionCodes.AllApiAccess)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var permissions = permissionCodes.Select(code =>
        {
            var (name, description) = PermissionMapper.GetPermissionInfo(code);
            return new RolePermission(code, name, description);
        });

        var adminRole = new Role("超级管理员", "平台默认管理员角色", permissions, DataScope.All);
        dbContext.Roles.Add(adminRole);

        var passwordHash = passwordHasher.Hash(DefaultAdminPassword);
        var adminUser = new User(
            DefaultAdminName,
            string.Empty,
            passwordHash,
            [new UserRole(adminRole.Id, adminRole.Name)],
            "系统管理员",
            1,
            "admin@local",
            string.Empty,
            DateTimeOffset.MinValue,
            UserId.Unassigned,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        adminUser.AssignDept(rootDept.Id, rootDept.Name);
        dbContext.Users.Add(adminUser);

        logger.LogInformation(
            "PlatformAdminSeeder: 已创建默认管理员 {UserName}，初始密码见项目 README",
            DefaultAdminName);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
