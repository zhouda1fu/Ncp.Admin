using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Utils;

/// <summary>
/// 数据库种子数据扩展方法
/// 用于在开发环境中初始化基础数据（角色、权限、组织架构、用户等）
/// </summary>
public static class SeedDatabaseExtension
{
    /// <summary>
    /// 初始化数据库种子数据
    /// 包括：角色和权限、组织架构、管理员用户、测试用户
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    internal static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 初始化角色和权限
        if (!dbContext.Roles.Any())
        {
            var adminPermissions = new List<RolePermission>
            {
                // 用户管理权限
                new RolePermission(PermissionCodes.UserCreate, "创建用户", "创建新用户"),
                new RolePermission(PermissionCodes.UserView, "查看用户", "查看用户信息"),
                new RolePermission(PermissionCodes.UserEdit, "更新用户", "更新用户信息"),
                new RolePermission(PermissionCodes.UserDelete, "删除用户", "删除用户"),
                new RolePermission(PermissionCodes.UserRoleAssign, "分配用户角色", "分配用户角色权限"),
                new RolePermission(PermissionCodes.UserResetPassword, "重置用户密码", "重置用户密码"),

                // 角色管理权限
                new RolePermission(PermissionCodes.RoleCreate, "创建角色", "创建新角色"),
                new RolePermission(PermissionCodes.RoleView, "查看角色", "查看角色信息"),
                new RolePermission(PermissionCodes.RoleEdit, "更新角色", "更新角色信息"),
                new RolePermission(PermissionCodes.RoleDelete, "删除角色", "删除角色"),
                new RolePermission(PermissionCodes.RoleUpdatePermissions, "更新角色权限", "更新角色的权限"),

                // 组织架构管理权限
                new RolePermission(PermissionCodes.OrganizationUnitCreate, "创建组织机构", "创建组织机构"),
                new RolePermission(PermissionCodes.OrganizationUnitView, "查看组织机构", "查看组织机构信息"),
                new RolePermission(PermissionCodes.OrganizationUnitEdit, "更新组织机构", "更新组织机构信息"),
                new RolePermission(PermissionCodes.OrganizationUnitDelete, "删除组织机构", "删除组织机构"),
                new RolePermission(PermissionCodes.OrganizationUnitAssign, "分配组织架构", "分配组织架构"),

                // 生产计划管理权限
                new RolePermission(PermissionCodes.ProductionPlanCreate, "创建生产计划", "创建新的生产计划"),
                new RolePermission(PermissionCodes.ProductionPlanView, "查看生产计划", "查看生产计划信息"),
                new RolePermission(PermissionCodes.ProductionPlanEdit, "更新生产计划", "更新生产计划信息"),
                new RolePermission(PermissionCodes.ProductionPlanDelete, "删除生产计划", "删除生产计划"),

                // 工单管理权限
                new RolePermission(PermissionCodes.WorkOrderCreate, "创建工单", "创建新的工单"),
                new RolePermission(PermissionCodes.WorkOrderView, "查看工单", "查看工单信息"),
                new RolePermission(PermissionCodes.WorkOrderEdit, "更新工单", "更新工单信息"),
                new RolePermission(PermissionCodes.WorkOrderDelete, "删除工单", "删除工单"),

                // 工艺路线管理权限
                new RolePermission(PermissionCodes.RoutingCreate, "创建工艺路线", "创建新的工艺路线"),
                new RolePermission(PermissionCodes.RoutingView, "查看工艺路线", "查看工艺路线信息"),
                new RolePermission(PermissionCodes.RoutingEdit, "更新工艺路线", "更新工艺路线信息"),
                new RolePermission(PermissionCodes.RoutingDelete, "删除工艺路线", "删除工艺路线"),

                // 系统管理权限
                new RolePermission(PermissionCodes.SystemAdmin, "系统管理员权限", "拥有系统管理员权限"),
                new RolePermission(PermissionCodes.SystemMonitor, "系统监控权限", "拥有系统监控权限"),

                // 所有接口访问权限
                new RolePermission(PermissionCodes.AllApiAccess, "所有接口访问权限", "所有接口访问权限"),
            };

            var userPermissions = new List<RolePermission>
            {
                new RolePermission(PermissionCodes.UserView, "查看用户", "查看用户信息"),
                new RolePermission(PermissionCodes.UserEdit, "更新用户", "更新自己的用户信息"),
                new RolePermission(PermissionCodes.ProductionPlanView, "查看生产计划", "查看生产计划信息"),
                new RolePermission(PermissionCodes.WorkOrderView, "查看工单", "查看工单信息"),
                new RolePermission(PermissionCodes.RoutingView, "查看工艺路线", "查看工艺路线信息"),
                new RolePermission(PermissionCodes.AllApiAccess, "所有接口访问权限", "所有接口访问权限"),
            };

            var adminRole = new Role("管理员", "系统管理员", adminPermissions);
            var userRole = new Role("普通用户", "普通用户", userPermissions);

            dbContext.Roles.Add(adminRole);
            dbContext.Roles.Add(userRole);
            dbContext.SaveChanges();
        }

        // 初始化组织机构
        if (!dbContext.OrganizationUnits.Any())
        {
            var organizationUnit = new OrganizationUnit("大组", "根节点", new OrganizationUnitId(0), 1);

            dbContext.OrganizationUnits.Add(organizationUnit);
            dbContext.SaveChanges();

            var organizationUnitId = dbContext.OrganizationUnits.First(r => r.Name == "大组").Id;
            var childGroup = new OrganizationUnit("小组", "第一个子节点", organizationUnitId, 1);
            dbContext.OrganizationUnits.Add(childGroup);
            dbContext.SaveChanges();

            var childGroupId = dbContext.OrganizationUnits.First(r => r.Name == "小组").Id;
            var childIndividual = new OrganizationUnit("个人", "第一个子节点的子节点", childGroupId, 1);
            dbContext.OrganizationUnits.Add(childIndividual);
            dbContext.SaveChanges();
        }

        // 初始化管理员用户
        if (!dbContext.Users.Any(u => u.Name == "admin"))
        {
            var organizationUnit = dbContext.OrganizationUnits.First(r => r.Name == "大组");
            var adminRole = dbContext.Roles.First(r => r.Name == "管理员");
            var adminUser = new User(
                "admin",
                "13800138000",
                PasswordHasher.HashPassword("123456"),
                new List<UserRole> { new UserRole(adminRole.Id, adminRole.Name) },
                "系统管理员",
                1,
                "admin@example.com",
                "男",
                DateTimeOffset.Now.AddYears(-30) // 假设管理员年龄为30岁
            );

            // 设置组织架构关系
            adminUser.AssignOrganizationUnit(new UserOrganizationUnit(adminUser.Id, organizationUnit.Id, organizationUnit.Name));
            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
        }

        // 初始化测试用户
        if (!dbContext.Users.Any(u => u.Name == "test"))
        {
            var organizationUnit = dbContext.OrganizationUnits.First(r => r.Name == "大组");
            var userRole = dbContext.Roles.First(r => r.Name == "普通用户");
            var testUser = new User(
                "test",
                "13800138001",
                PasswordHasher.HashPassword("123456"),
                new List<UserRole> { new UserRole(userRole.Id, userRole.Name) },
                "测试用户",
                1,
                "test@example.com",
                "女",
                DateTimeOffset.Now.AddYears(-25) // 假设测试用户年龄为25岁
            );

            testUser.AssignOrganizationUnit(new UserOrganizationUnit(testUser.Id, organizationUnit.Id, organizationUnit.Name));
            dbContext.Users.Add(testUser);
            dbContext.SaveChanges();
        }

        return app;
    }
}

