using Microsoft.AspNetCore.Hosting;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.AppPermissions;
using Serilog;

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
        try
        {
            Log.Information("开始初始化数据库种子数据...");
            using var serviceScope = app.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordHasher = serviceScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var env = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // 初始化角色和权限
            if (!dbContext.Roles.Any())
            {
                var adminPermissionCodes = new[]
                {
                // 父权限码（用于菜单和路由权限控制）
                PermissionCodes.UserManagement,
                PermissionCodes.RoleManagement,
                PermissionCodes.DeptManagement,

                // 用户管理权限
                PermissionCodes.UserCreate,
                PermissionCodes.UserView,
                PermissionCodes.UserEdit,
                PermissionCodes.UserDelete,
                PermissionCodes.UserRoleAssign,
                PermissionCodes.UserResetPassword,

                // 角色管理权限
                PermissionCodes.RoleCreate,
                PermissionCodes.RoleView,
                PermissionCodes.RoleEdit,
                PermissionCodes.RoleDelete,
                PermissionCodes.RoleUpdatePermissions,

                // 部门管理权限
                PermissionCodes.DeptCreate,
                PermissionCodes.DeptView,
                PermissionCodes.DeptEdit,
                PermissionCodes.DeptDelete,

                // 岗位管理权限
                PermissionCodes.PositionManagement,
                PermissionCodes.PositionCreate,
                PermissionCodes.PositionView,
                PermissionCodes.PositionEdit,
                PermissionCodes.PositionDelete,

                // 通知管理权限
                PermissionCodes.NotificationManagement,
                PermissionCodes.NotificationView,
                PermissionCodes.NotificationSend,

                // 工作流管理权限
                PermissionCodes.WorkflowManagement,
                PermissionCodes.WorkflowDefinitionView,
                PermissionCodes.WorkflowDefinitionCreate,
                PermissionCodes.WorkflowDefinitionEdit,
                PermissionCodes.WorkflowDefinitionDelete,
                PermissionCodes.WorkflowDefinitionPublish,
                PermissionCodes.WorkflowStart,
                PermissionCodes.WorkflowCancel,
                PermissionCodes.WorkflowTaskApprove,
                PermissionCodes.WorkflowInstanceView,
                PermissionCodes.WorkflowMonitor,

                // 客户管理权限
                PermissionCodes.CustomerManagement,
                PermissionCodes.CustomerView,
                PermissionCodes.CustomerCreate,
                PermissionCodes.CustomerEdit,
                PermissionCodes.CustomerDelete,
                PermissionCodes.CustomerContactEdit,
                PermissionCodes.CustomerReleaseToSea,
                PermissionCodes.CustomerClaimFromSea,
                PermissionCodes.IndustryView,
                PermissionCodes.IndustryCreate,
                PermissionCodes.IndustryEdit,
                PermissionCodes.CustomerSourceView,
                PermissionCodes.CustomerSourceCreate,
                PermissionCodes.CustomerSourceEdit,
                PermissionCodes.RegionView,
                PermissionCodes.RegionCreate,
                PermissionCodes.RegionEdit,

                // 订单管理权限
                PermissionCodes.OrderManagement,
                PermissionCodes.OrderView,
                PermissionCodes.OrderCreate,
                PermissionCodes.OrderEdit,
                PermissionCodes.OrderDelete,

                // 产品管理权限
                PermissionCodes.ProductManagement,
                PermissionCodes.ProductView,
                PermissionCodes.ProductCreate,
                PermissionCodes.ProductEdit,
                PermissionCodes.ProductDelete,

                // 所有接口访问权限
                PermissionCodes.AllApiAccess,
            };

                var adminPermissions = adminPermissionCodes.Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    return new RolePermission(code, name, description);
                }).ToList();

                var userPermissionCodes = new[]
                {
                PermissionCodes.UserView,
                PermissionCodes.UserEdit,
                PermissionCodes.AllApiAccess,
            };

                var userPermissions = userPermissionCodes.Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    // 对于 UserEdit，使用特殊描述
                    if (code == PermissionCodes.UserEdit)
                    {
                        description = "更新自己的用户信息";
                    }
                    return new RolePermission(code, name, description);
                }).ToList();

                var adminRole = new Role("管理员", "系统管理员", adminPermissions);
                var userRole = new Role("普通用户", "普通用户", userPermissions);

                dbContext.Roles.Add(adminRole);
                dbContext.Roles.Add(userRole);
                dbContext.SaveChanges();
            }

            // 初始化部门
            if (!dbContext.Depts.Any())
            {
                var dept = new Dept("研发", "根节点", new DeptId(0), 1);

                dbContext.Depts.Add(dept);
                dbContext.SaveChanges();

                var deptId = dbContext.Depts.FirstOrDefault(r => r.Name == "研发")?.Id;
                if (deptId == null)
                {
                    throw new InvalidOperationException("无法找到部门'研发'，请确保部门已正确创建");
                }
                var childGroup = new Dept("Net", "第一个子节点", deptId, 1);
                dbContext.Depts.Add(childGroup);
                dbContext.SaveChanges();

                var childGroupId = dbContext.Depts.FirstOrDefault(r => r.Name == "Net")?.Id;
                if (childGroupId == null)
                {
                    throw new InvalidOperationException("无法找到部门'Net'，请确保部门已正确创建");
                }
                var childIndividual = new Dept("C#", "第一个子节点的子节点", childGroupId, 1);
                dbContext.Depts.Add(childIndividual);
                dbContext.SaveChanges();
            }

            // 初始化合同签订公司（合同类型选项），第一项为默认值（SortOrder 最小）
            if (!dbContext.ContractTypeOptions.Any())
            {
                var contractTypeOptions = new[]
                {
                    new ContractTypeOption("安徽阳光心健科技发展有限公司第三分公司", 1, true, 0),
                    new ContractTypeOption("安徽阳光心健科技发展有限公司第五分公司", 2, true, 1),
                    new ContractTypeOption("安徽阳光心健科技发展有限公司", 3, true, 2),
                    new ContractTypeOption("安徽百睿特新材料科技有限公司", 4, true, 3),
                    new ContractTypeOption("安徽惠明教育科技有限公司", 5, true, 4),
                    new ContractTypeOption("合肥楚一贸易有限公司", 6, true, 5),
                    new ContractTypeOption("合肥市新晨心理咨询有限公司", 7, true, 6),
                    new ContractTypeOption("安徽乐康教育装备有限公司", 8, true, 7),
                    new ContractTypeOption("合肥市叶懋智能科技有限公司", 9, true, 8),
                };
                foreach (var opt in contractTypeOptions)
                    dbContext.ContractTypeOptions.Add(opt);
                dbContext.SaveChanges();
            }

            // 初始化收支类型选项，第一项为默认值（SortOrder 最小）
            if (!dbContext.IncomeExpenseTypeOptions.Any())
            {
                var incomeExpenseTypeOptions = new[]
                {
                    new IncomeExpenseTypeOption("对公转款", 1, 0),
                    new IncomeExpenseTypeOption("微信", 2, 1),
                    new IncomeExpenseTypeOption("支付宝", 3, 2),
                    new IncomeExpenseTypeOption("现金", 4, 3),
                };
                foreach (var opt in incomeExpenseTypeOptions)
                    dbContext.IncomeExpenseTypeOptions.Add(opt);
                dbContext.SaveChanges();
            }

            // 初始化项目状态选项：新项目、跟进中、已挂网、项目暂停、中标待采、黄了
            if (!dbContext.ProjectStatusOptions.Any())
            {
                var projectStatusOptions = new[]
                {
                    new ProjectStatusOption("新项目", "new", 0),
                    new ProjectStatusOption("跟进中", "following", 1),
                    new ProjectStatusOption("已挂网", "listed", 2),
                    new ProjectStatusOption("项目暂停", "paused", 3),
                    new ProjectStatusOption("中标待采", "won", 4),
                    new ProjectStatusOption("黄了", "failed", 5),
                };
                foreach (var opt in projectStatusOptions)
                    dbContext.ProjectStatusOptions.Add(opt);
                dbContext.SaveChanges();
            }

            // 初始化项目类型：小型、中型、大型
            if (!dbContext.ProjectTypes.Any())
            {
                var projectTypes = new[]
                {
                    new ProjectType("小型", 0),
                    new ProjectType("中型", 1),
                    new ProjectType("大型", 2),
                };
                foreach (var t in projectTypes)
                    dbContext.ProjectTypes.Add(t);
                dbContext.SaveChanges();
            }

            // 初始化项目行业：司法系统、检法系统、教育系统、部队、企事业单位、卫生系统、公安系统、其他
            if (!dbContext.ProjectIndustries.Any())
            {
                var projectIndustries = new[]
                {
                    new ProjectIndustry("司法系统", 0),
                    new ProjectIndustry("检法系统", 1),
                    new ProjectIndustry("教育系统", 2),
                    new ProjectIndustry("部队", 3),
                    new ProjectIndustry("企事业单位", 4),
                    new ProjectIndustry("卫生系统", 5),
                    new ProjectIndustry("公安系统", 6),
                    new ProjectIndustry("其他", 7),
                };
                foreach (var ind in projectIndustries)
                    dbContext.ProjectIndustries.Add(ind);
                dbContext.SaveChanges();
            }

            // 初始化客户来源（方案 A：按使用场景区分，公海 / 客户列表 / 通用）
            if (!dbContext.CustomerSources.Any())
            {
                var sea = CustomerSourceUsageScene.Sea;
                var list = CustomerSourceUsageScene.List;
                var both = CustomerSourceUsageScene.Both;
                var customerSources = new[]
                {
                    // 公海侧
                    new CustomerSource("爱番番", 0, sea),
                    new CustomerSource("网站直接访问", 1, sea),
                    new CustomerSource("360搜索", 2, sea),
                    new CustomerSource("百度自然搜索", 3, sea),
                    new CustomerSource("非搜索引擎来源", 4, sea),
                    new CustomerSource("百度搜索推广-基木鱼", 5, sea),
                    new CustomerSource("微软必应", 6, sea),
                    new CustomerSource("百度其他-基木鱼", 7, sea),
                    new CustomerSource("谷歌(.com)", 8, sea),
                    new CustomerSource("神马搜索", 9, sea),
                    new CustomerSource("搜狗", 10, sea),
                    new CustomerSource("天猫", 11, sea),
                    new CustomerSource("淘宝", 12, sea),
                    new CustomerSource("京东", 13, sea),
                    new CustomerSource("抖音", 14, sea),
                    new CustomerSource("400电话", 15, sea),
                    new CustomerSource("阿里巴巴", 16, sea),
                    new CustomerSource("小红书", 17, sea),
                    new CustomerSource("其他", 18, sea),
                    // 列表侧
                    new CustomerSource("网络推广", 19, list),
                    new CustomerSource("陌生开发", 20, list),
                    new CustomerSource("同事分享", 21, list),
                    new CustomerSource("客户公海", 22, list),
                    // 通用（两处都显示）
                    new CustomerSource("展会", 23, both),
                    new CustomerSource("客户介绍", 24, both),
                };
                foreach (var source in customerSources)
                    dbContext.CustomerSources.Add(source);
                dbContext.SaveChanges();
            }

            // 初始化行业主数据（从 Utils/SeedDatas/IndustrySeed.txt 读取：第一部分父级，第二部分子级）
            if (!dbContext.Industries.Any())
            {
                var industrySeedPath = Path.Combine(env.ContentRootPath, "Utils", "SeedDatas", "IndustrySeed.txt");
                var (parentRows, childRows) = GetIndustrySeedLinesFromFile(industrySeedPath);
                if (parentRows.Count > 0)
                {
                    var parentList = parentRows.OrderBy(x => x.SortOrder).ToList();
                    foreach (var p in parentList)
                    {
                        var industry = new Industry(p.Name, null, p.SortOrder, null);
                        dbContext.Industries.Add(industry);
                    }
                    dbContext.SaveChanges();
                    var parentIdByOldPk = new Dictionary<int, IndustryId>();
                    var savedParents = dbContext.Industries.Where(x => x.ParentId == null).OrderBy(x => x.SortOrder).ToList();
                    for (var i = 0; i < parentList.Count && i < savedParents.Count; i++)
                        parentIdByOldPk[parentList[i].Pk] = savedParents[i].Id;
                    foreach (var c in childRows)
                    {
                        if (string.IsNullOrWhiteSpace(c.Title)) continue;
                        IndustryId? parentId = null;
                        if (c.FkIndustry > 0 && parentIdByOldPk.TryGetValue(c.FkIndustry, out var pid))
                            parentId = pid;
                        var child = new Industry(c.Title.Trim(), parentId, c.SortOrder, string.IsNullOrWhiteSpace(c.Remark) ? null : c.Remark.Trim());
                        dbContext.Industries.Add(child);
                    }
                    dbContext.SaveChanges();
                }
            }

            // 初始化区域主数据（从 Utils/SeedDatas/RegionSeed.txt 读取：Code, Name, Parent_Code, Level）
            if (!dbContext.Regions.Any())
            {
                var seedPath = Path.Combine(env.ContentRootPath, "Utils", "SeedDatas", "RegionSeed.txt");
                var regionLines = GetRegionSeedLinesFromFile(seedPath);
                foreach (var (code, name, parentCode, level) in regionLines)
                {
                    var region = new Region(
                        new RegionId(code),
                        name,
                        new RegionId(parentCode),
                        level,
                        sortOrder: 0);
                    dbContext.Regions.Add(region);
                }
                dbContext.SaveChanges();
            }

            // 初始化管理员用户
            if (!dbContext.Users.Any(u => u.Name == "admin"))
            {
                var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
                var adminRole = dbContext.Roles.FirstOrDefault(r => r.Name == "管理员");

                if (dept == null)
                {
                    throw new InvalidOperationException("无法找到部门'研发'，请确保部门已正确初始化");
                }

                if (adminRole == null)
                {
                    throw new InvalidOperationException("无法找到角色'管理员'，请确保角色已正确初始化");
                }

                var adminUser = new User(
                    "admin",
                    "13800138000",
                    passwordHasher.Hash("123456"),
                    new List<UserRole> { new UserRole(adminRole.Id, adminRole.Name) },
                    "系统管理员",
                    1,
                    "admin@example.com",
                    "男",
                   DateTimeOffset.UtcNow.AddYears(-30) // 假设管理员年龄为30岁
                );

                // 设置部门关系
                adminUser.AssignDept(new UserDept(adminUser.Id, dept.Id, dept.Name));
                dbContext.Users.Add(adminUser);
                dbContext.SaveChanges();
            }

            // 初始化测试用户
            if (!dbContext.Users.Any(u => u.Name == "test"))
            {
                var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
                var userRole = dbContext.Roles.FirstOrDefault(r => r.Name == "普通用户");

                if (dept == null)
                {
                    throw new InvalidOperationException("无法找到部门'研发'，请确保部门已正确初始化");
                }

                if (userRole == null)
                {
                    throw new InvalidOperationException("无法找到角色'普通用户'，请确保角色已正确初始化");
                }

                var testUser = new User(
                    "test",
                    "13800138001",
                    passwordHasher.Hash("123456"),
                    new List<UserRole> { new UserRole(userRole.Id, userRole.Name) },
                    "测试用户",
                    1,
                    "test@example.com",
                    "女",
                   DateTimeOffset.UtcNow.AddYears(-25) // 假设测试用户年龄为25岁
                );

                testUser.AssignDept(new UserDept(testUser.Id, dept.Id, dept.Name));
                dbContext.Users.Add(testUser);
                dbContext.SaveChanges();
            }

            Log.Information("数据库种子数据初始化完成");
            return app;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "数据库种子数据初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 从 IndustrySeed.txt 读取行业种子数据。第一部分：pk_Industry, Title, OrderNum（父级）；第二部分：fk_Industry, Title, OrderNum, Remark（子级）。
    /// </summary>
    private static (List<(int Pk, string Name, int SortOrder)> Parents, List<(int FkIndustry, string Title, int SortOrder, string Remark)> Children) GetIndustrySeedLinesFromFile(string filePath)
    {
        var parents = new List<(int Pk, string Name, int SortOrder)>();
        var children = new List<(int FkIndustry, string Title, int SortOrder, string Remark)>();
        if (!File.Exists(filePath))
        {
            Log.Warning("行业种子文件不存在，跳过行业初始化: {Path}", filePath);
            return (parents, children);
        }
        var lines = File.ReadAllLines(filePath);
        var inParent = false;
        var inChild = false;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                inParent = false;
                inChild = false;
                continue;
            }
            if (line.StartsWith("pk_Industry\t", StringComparison.OrdinalIgnoreCase))
            {
                inParent = true;
                inChild = false;
                continue;
            }
            if (line.StartsWith("pk_Item\t", StringComparison.OrdinalIgnoreCase))
            {
                inParent = false;
                inChild = true;
                continue;
            }
            var parts = line.Split('\t');
            if (inParent && parts.Length >= 4 && int.TryParse(parts[0], out var pk) && int.TryParse(parts[3], out var pOrder))
                parents.Add((pk, parts[1].Trim(), pOrder));
            if (inChild && parts.Length >= 5 && int.TryParse(parts[1], out var fk) && int.TryParse(parts[4], out var cOrder))
                children.Add((fk, parts[2].Trim(), cOrder, parts.Length > 5 ? parts[5].Trim() : ""));
        }
        return (parents, children);
    }

    /// <summary>
    /// 从 RegionSeed.txt 读取区域种子数据（格式：Code\tName\tParent_Code\tLevel，首行为表头）
    /// </summary>
    private static IEnumerable<(long Code, string Name, long ParentCode, int Level)> GetRegionSeedLinesFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Log.Warning("区域种子文件不存在，跳过区域初始化: {Path}", filePath);
            yield break;
        }
        var lines = File.ReadAllLines(filePath);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            // 跳过表头
            if (i == 0 && (line.StartsWith("Code", StringComparison.OrdinalIgnoreCase) || line.StartsWith("code")))
                continue;
            var parts = line.Split('\t');
            if (parts.Length != 4) continue;
            if (!long.TryParse(parts[0], out var code) || !long.TryParse(parts[2], out var parentCode) || !int.TryParse(parts[3], out var level))
                continue;
            yield return (code, parts[1], parentCode, level);
        }
    }
}


