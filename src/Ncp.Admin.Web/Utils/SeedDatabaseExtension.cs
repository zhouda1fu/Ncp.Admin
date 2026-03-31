using Microsoft.AspNetCore.Hosting;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptions;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
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
                PermissionCodes.UserExport,
                PermissionCodes.UserImport,

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
                PermissionCodes.CustomerShare,
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
                PermissionCodes.OrderSubmit,

                // 产品管理权限
                PermissionCodes.ProductManagement,
                PermissionCodes.ProductView,
                PermissionCodes.ProductCreate,
                PermissionCodes.ProductEdit,
                PermissionCodes.ProductDelete,

                // 操作日志权限
                PermissionCodes.OperationLogManagement,
                PermissionCodes.OperationLogView,

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
                    if (code == PermissionCodes.UserEdit)
                        description = "更新自己的用户信息";
                    return new RolePermission(code, name, description);
                }).ToList();

                // 部门经理：查看组织与人员、考勤、请假/报销审批、任务、工作流待办
                var deptManagerPermissionCodes = new[]
                {
                    PermissionCodes.UserView,
                    PermissionCodes.DeptView,
                    PermissionCodes.PositionView,
                    PermissionCodes.WorkflowTaskApprove,
                    PermissionCodes.WorkflowInstanceView,
                    PermissionCodes.AttendanceManagement,
                    PermissionCodes.AttendanceRecordView,
                    PermissionCodes.ScheduleView,
                    PermissionCodes.ExpenseClaimView,
                    PermissionCodes.ExpenseClaimSubmit,
                    PermissionCodes.TaskManagement,
                    PermissionCodes.ProjectView,
                    PermissionCodes.TaskView,
                    PermissionCodes.LeaveRequestView,
                    PermissionCodes.LeaveRequestCreate,
                    PermissionCodes.AnnouncementView,
                    PermissionCodes.AllApiAccess,
                };
                var deptManagerPermissions = deptManagerPermissionCodes.Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    return new RolePermission(code, name, description);
                }).ToList();

                // 人事专员：用户/部门/岗位的查看与编辑（不含删除）、组织与人员
                var hrPermissionCodes = new[]
                {
                    PermissionCodes.UserManagement,
                    PermissionCodes.UserView,
                    PermissionCodes.UserCreate,
                    PermissionCodes.UserEdit,
                    PermissionCodes.UserRoleAssign,
                    PermissionCodes.UserExport,
                    PermissionCodes.UserImport,
                    PermissionCodes.DeptManagement,
                    PermissionCodes.DeptView,
                    PermissionCodes.DeptCreate,
                    PermissionCodes.DeptEdit,
                    PermissionCodes.PositionManagement,
                    PermissionCodes.PositionView,
                    PermissionCodes.PositionCreate,
                    PermissionCodes.PositionEdit,
                    PermissionCodes.AnnouncementView,
                    PermissionCodes.AllApiAccess,
                };
                var hrPermissions = hrPermissionCodes.Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    return new RolePermission(code, name, description);
                }).ToList();

                // 财务：报销、订单查看与审批相关
                var financePermissionCodes = new[]
                {
                    PermissionCodes.UserView,
                    PermissionCodes.ExpenseManagement,
                    PermissionCodes.ExpenseClaimView,
                    PermissionCodes.ExpenseClaimCreate,
                    PermissionCodes.ExpenseClaimSubmit,
                    PermissionCodes.OrderManagement,
                    PermissionCodes.OrderView,
                    PermissionCodes.WorkflowTaskApprove,
                    PermissionCodes.WorkflowInstanceView,
                    PermissionCodes.AllApiAccess,
                };
                var financePermissions = financePermissionCodes.Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    return new RolePermission(code, name, description);
                }).ToList();

                // 行政：公告、会议、资产、用车、文档等
                var adminStaffPermissionCodes = new[]
                {
                    PermissionCodes.UserView,
                    PermissionCodes.AnnouncementManagement,
                    PermissionCodes.AnnouncementView,
                    PermissionCodes.AnnouncementCreate,
                    PermissionCodes.AnnouncementEdit,
                    PermissionCodes.AnnouncementPublish,
                    PermissionCodes.MeetingManagement,
                    PermissionCodes.MeetingRoomView,
                    PermissionCodes.MeetingRoomEdit,
                    PermissionCodes.MeetingBookingView,
                    PermissionCodes.MeetingBookingCreate,
                    PermissionCodes.AssetManagement,
                    PermissionCodes.AssetView,
                    PermissionCodes.AssetCreate,
                    PermissionCodes.AssetEdit,
                    PermissionCodes.AssetAllocate,
                    PermissionCodes.AssetReturn,
                    PermissionCodes.VehicleManagement,
                    PermissionCodes.VehicleView,
                    PermissionCodes.VehicleBookingView,
                    PermissionCodes.VehicleBookingCreate,
                    PermissionCodes.DocumentManagement,
                    PermissionCodes.DocumentView,
                    PermissionCodes.DocumentCreate,
                    PermissionCodes.DocumentEdit,
                    PermissionCodes.NotificationView,
                    PermissionCodes.AllApiAccess,
                };
                var adminStaffPermissions = adminStaffPermissionCodes.Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    return new RolePermission(code, name, description);
                }).ToList();

                // 审批人：仅工作流待办与实例查看
                var approverPermissionCodes = new[]
                {
                    PermissionCodes.UserView,
                    PermissionCodes.WorkflowTaskApprove,
                    PermissionCodes.WorkflowInstanceView,
                    PermissionCodes.AllApiAccess,
                };
                var approverPermissions = approverPermissionCodes.Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    return new RolePermission(code, name, description);
                }).ToList();

                var adminRole = new Role("管理员", "系统管理员，拥有全部权限", adminPermissions);
                var userRole = new Role("普通用户", "普通用户，可查看和编辑自己的信息", userPermissions);
                var deptManagerRole = new Role("部门经理", "部门经理，可查看本部门人员、考勤、审批请假与报销等", deptManagerPermissions);
                var hrRole = new Role("人事专员", "人事专员，负责用户、部门、岗位的维护", hrPermissions);
                var financeRole = new Role("财务", "财务人员，负责报销与订单相关", financePermissions);
                var adminStaffRole = new Role("行政", "行政人员，负责公告、会议、资产、用车、文档等", adminStaffPermissions);
                var approverRole = new Role("审批人", "审批人，可处理工作流待办任务", approverPermissions);

                dbContext.Roles.Add(adminRole);
                dbContext.Roles.Add(userRole);
                dbContext.Roles.Add(deptManagerRole);
                dbContext.Roles.Add(hrRole);
                dbContext.Roles.Add(financeRole);
                dbContext.Roles.Add(adminStaffRole);
                dbContext.Roles.Add(approverRole);
                dbContext.SaveChanges();
            }

            // 初始化部门（OA 标准：公司根 -> 一级部门 -> 二级部门；主管ID占位为0，后续可在部门管理中维护）
            if (!dbContext.Depts.Any())
            {
                var managerIdPlaceholder = new UserId(0);
                var rootDept = new Dept("公司", "根节点", new DeptId(0), 1, managerIdPlaceholder);
                dbContext.Depts.Add(rootDept);
                dbContext.SaveChanges();

                var companyId = dbContext.Depts.FirstOrDefault(r => r.Name == "公司")?.Id
                    ?? throw new InvalidOperationException("无法找到部门'公司'");

                var level1Depts = new[]
                {
                    ("总经办", "总经理办公室"),
                    ("研发中心", "技术研发部门"),
                    ("市场部", "市场与销售"),
                    ("人力资源部", "人事与招聘"),
                    ("财务部", "财务与核算"),
                    ("行政部", "行政与后勤"),
                };
                foreach (var (name, remark) in level1Depts)
                {
                    dbContext.Depts.Add(new Dept(name, remark, companyId, 1, managerIdPlaceholder));
                }
                dbContext.SaveChanges();

                var rdDeptId = dbContext.Depts.FirstOrDefault(r => r.Name == "研发中心")?.Id
                    ?? throw new InvalidOperationException("无法找到部门'研发中心'");
                dbContext.Depts.Add(new Dept("前端组", "前端开发组", rdDeptId, 1, managerIdPlaceholder));
                dbContext.Depts.Add(new Dept("后端组", "后端开发组", rdDeptId, 1, managerIdPlaceholder));
                dbContext.SaveChanges();
            }

            // 初始化岗位（按部门挂载，OA 常见岗位）
            if (!dbContext.Positions.Any())
            {
                DeptId GetDeptId(string name) =>
                    dbContext.Depts.FirstOrDefault(d => d.Name == name)?.Id
                    ?? throw new InvalidOperationException($"无法找到部门'{name}'");

                var jb = GetDeptId("总经办");
                dbContext.Positions.Add(new Position("总经理", "GM", "公司总经理", jb, 0, 1));
                dbContext.Positions.Add(new Position("总经理助理", "GMA", "总经理助理", jb, 1, 1));

                var rd = GetDeptId("研发中心");
                dbContext.Positions.Add(new Position("技术总监", "CTO", "技术负责人", rd, 0, 1));
                dbContext.Positions.Add(new Position("高级工程师", "SE", "高级开发工程师", rd, 1, 1));
                dbContext.Positions.Add(new Position("工程师", "ENG", "开发工程师", rd, 2, 1));
                dbContext.Positions.Add(new Position("实习生", "INTERN", "实习岗位", rd, 3, 1));

                var fe = GetDeptId("前端组");
                dbContext.Positions.Add(new Position("前端工程师", "FE", "前端开发", fe, 0, 1));
                var be = GetDeptId("后端组");
                dbContext.Positions.Add(new Position("后端工程师", "BE", "后端开发", be, 0, 1));

                var mkt = GetDeptId("市场部");
                dbContext.Positions.Add(new Position("市场经理", "MM", "市场部负责人", mkt, 0, 1));
                dbContext.Positions.Add(new Position("市场专员", "MS", "市场专员", mkt, 1, 1));

                var hr = GetDeptId("人力资源部");
                dbContext.Positions.Add(new Position("HR经理", "HRM", "人力资源经理", hr, 0, 1));
                dbContext.Positions.Add(new Position("HR专员", "HRS", "人力资源专员", hr, 1, 1));
                dbContext.Positions.Add(new Position("招聘专员", "REC", "招聘岗位", hr, 2, 1));

                var fin = GetDeptId("财务部");
                dbContext.Positions.Add(new Position("财务经理", "FM", "财务负责人", fin, 0, 1));
                dbContext.Positions.Add(new Position("会计", "ACC", "会计", fin, 1, 1));
                dbContext.Positions.Add(new Position("出纳", "CASH", "出纳", fin, 2, 1));

                var adminDept = GetDeptId("行政部");
                dbContext.Positions.Add(new Position("行政经理", "AM", "行政负责人", adminDept, 0, 1));
                dbContext.Positions.Add(new Position("行政专员", "AS", "行政专员", adminDept, 1, 1));

                dbContext.SaveChanges();
            }

            // 初始化合同签订公司（合同类型选项），第一项为默认值（SortOrder 最小）
            if (!dbContext.ContractTypeOptions.Any())
            {
                var contractTypeOptions = new[]
                {
                    new ContractTypeOption("示例签约主体A有限公司", 1, true, 0),
                    new ContractTypeOption("示例签约主体B有限公司", 2, true, 1),
                    new ContractTypeOption("示例签约主体C有限公司", 3, true, 2),
                    new ContractTypeOption("示例签约主体D有限公司", 4, true, 3),
                    new ContractTypeOption("示例签约主体E有限公司", 5, true, 4),
                    new ContractTypeOption("示例签约主体F有限公司", 6, true, 5),
                    new ContractTypeOption("示例签约主体G有限公司", 7, true, 6),
                    new ContractTypeOption("示例签约主体H有限公司", 8, true, 7),
                    new ContractTypeOption("示例签约主体I有限公司", 9, true, 8),
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

            // 初始化物流公司
            if (!dbContext.OrderLogisticsCompanies.Any())
            {
                var logisticsCompanies = new[]
                {
                    (1, "顺丰速运", 1),
                    (2, "百世快运", 2),
                    (3, "城市100", 3),
                    (4, "德邦", 4),
                    (5, "EMS", 5),
                    (6, "快捷速递", 6),
                    (7, "FEDEX联邦(国内件）", 7),
                    (8, "FEDEX联邦(国际件）", 8),
                    (9, "国通快递", 9),
                    (10, "汇丰物流", 10),
                    (11, "天天快递", 11),
                    (12, "天地华宇", 12),
                    (13, "百世快递", 13),
                    (14, "急先达", 14),
                    (15, "龙邦快递", 15),
                    (16, "全日通快递", 16),
                    (17, "如风达", 17),
                    (18, "盛邦物流", 18),
                    (19, "盛丰物流", 19),
                    (20, "速通物流", 20),
                    (21, "申通快递", 21),
                    (22, "速尔快递", 22),
                    (23, "全一快递", 23),
                    (24, "优速快递", 24),
                    (25, "信丰快递", 25),
                    (26, "源安达快递", 26),
                    (27, "韵达快递", 27),
                    (28, "义达国际物流", 28),
                    (29, "亚风快递", 29),
                    (30, "运通快递", 30),
                    (31, "圆通速递", 31),
                    (32, "邮政平邮/小包", 32),
                    (33, "宅急送", 33),
                    (34, "中通速递", 34),
                    (35, "亚马逊物流", 35),
                    (36, "速必达物流", 36),
                    (37, "城际快递", 37),
                    (38, "全峰快递", 38),
                    (40, "递四方速递", 39),
                };
                foreach (var (typeValue, name, sort) in logisticsCompanies)
                {
                    var company = OrderLogisticsCompany.Create(name, typeValue, sort);
                    dbContext.OrderLogisticsCompanies.Add(company);
                }
                dbContext.SaveChanges();
            }

            // 初始化物流方式
            if (!dbContext.OrderLogisticsMethods.Any())
            {
                var logisticsMethods = new[]
                {
                    (0, "物流发货", 0),
                    (1, "物流发货（含上楼）", 1),
                    (2, "快递发货", 2),
                    (3, "整车/拼车发货", 3),
                    (4, "整车/拼车发货（含上楼）", 4),
                    (5, "仓库自提", 5),
                    (6, "远程安装", 6),
                    (7, "仓库送货上门", 7),
                    (8, "汽运", 8),
                    (9, "其他", 9),
                };
                foreach (var (typeValue, name, sort) in logisticsMethods)
                {
                    var method = OrderLogisticsMethod.Create(name, typeValue, sort);
                    dbContext.OrderLogisticsMethods.Add(method);
                }
                dbContext.SaveChanges();
            }

            // 初始化订单发票类型
            if (!dbContext.OrderInvoiceTypeOptions.Any())
            {
                var invoiceTypes = new[]
                {
                    (0, "增值税专用发票", 0),
                    (1, "增值税普通发票", 1),
                    (2, "收据", 2),
                    (3, "航空运输电子客票行程单", 3),
                    (4, "铁路电子客票", 4),
                };
                foreach (var (typeValue, name, sortOrder) in invoiceTypes)
                {
                    var invoiceType = new OrderInvoiceTypeOption(name, typeValue, sortOrder);
                    dbContext.OrderInvoiceTypeOptions.Add(invoiceType);
                }
                dbContext.SaveChanges();
            }

            // 初始化产品分类
            if (!dbContext.ProductCategories.Any())
            {
                var rootParentId = new ProductCategoryId(Guid.Empty);
                var productCategories = new[]
                {
                    ("阳光心健", "阳光心健产品分类", rootParentId, 0, true),
                    ("微心理", "微心理产品分类", rootParentId, 1, true),
                };
                foreach (var (name, remark, parentId, sortOrder, visible) in productCategories)
                {
                    var category = new ProductCategory(name, remark, parentId, sortOrder, visible);
                    dbContext.ProductCategories.Add(category);
                }
                dbContext.SaveChanges();
            }

            // 初始化产品类型
            if (!dbContext.ProductTypes.Any())
            {
                var productTypes = new[]
                {
                    ("自有产品", 0, true),
                    ("外采产品", 1, true),
                    ("定制产品", 2, true),
                };
                foreach (var (name, sortOrder, visible) in productTypes)
                {
                    var type = new ProductType(name, sortOrder, visible);
                    dbContext.ProductTypes.Add(type);
                }
                dbContext.SaveChanges();
            }

            // 初始化产品供应商
            if (!dbContext.Suppliers.Any())
            {
                var supplier = new Supplier(
                    fullName: "示例供应商A",
                    shortName: "示例供应商",
                    contact: "示例联系人",
                    phone: "13000000010",
                    email: "",
                    address: "",
                    remark: ""
                );
                dbContext.Suppliers.Add(supplier);
                dbContext.SaveChanges();
            }

            // 初始化管理员用户（研发中心 + 技术总监岗位）
            if (!dbContext.Users.Any(u => u.Name == "admin"))
            {
                var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发中心");
                var adminRole = dbContext.Roles.FirstOrDefault(r => r.Name == "管理员");
                var position = dbContext.Positions.FirstOrDefault(p => p.Code == "CTO");

                if (dept == null)
                    throw new InvalidOperationException("无法找到部门'研发中心'，请确保部门已正确初始化");
                if (adminRole == null)
                    throw new InvalidOperationException("无法找到角色'管理员'，请确保角色已正确初始化");

                var adminUser = new User(
                    "admin",
                    "13800138000",
                    passwordHasher.Hash("123456"),
                    new List<UserRole> { new UserRole(adminRole.Id, adminRole.Name) },
                    "系统管理员",
                    1,
                    "admin@example.com",
                    "男",
                    DateTimeOffset.UtcNow.AddYears(-30),
                    new UserId(0),
                    "110101199001010000",
                    "地址",
                    "本科",
                    "毕业院校",
                    "https://example.com/avatar.png"
                );
                adminUser.AssignDept(new UserDept(adminUser.Id, dept.Id, dept.Name));
                dbContext.Users.Add(adminUser);
                dbContext.SaveChanges();
                if (position != null)
                {
                    adminUser.AssignPosition(new UserPosition(adminUser.Id, position.Id, position.Name));
                    dbContext.SaveChanges();
                }
            }

            // 初始化测试用户（研发中心 + 工程师岗位）
            if (!dbContext.Users.Any(u => u.Name == "test"))
            {
                var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发中心");
                var userRole = dbContext.Roles.FirstOrDefault(r => r.Name == "普通用户");
                var position = dbContext.Positions.FirstOrDefault(p => p.Code == "ENG");

                if (dept == null)
                    throw new InvalidOperationException("无法找到部门'研发中心'，请确保部门已正确初始化");
                if (userRole == null)
                    throw new InvalidOperationException("无法找到角色'普通用户'，请确保角色已正确初始化");

                var testUser = new User(
                    "test",
                    "13800138001",
                    passwordHasher.Hash("123456"),
                    new List<UserRole> { new UserRole(userRole.Id, userRole.Name) },
                    "测试用户",
                    1,
                    "test@example.com",
                    "女",
                    DateTimeOffset.UtcNow.AddYears(-25),
                    new UserId(0),
                    "110101199001010000",
                    "地址",
                    "本科",
                    "毕业院校",
                    "https://example.com/avatar.png"
                );
                testUser.AssignDept(new UserDept(testUser.Id, dept.Id, dept.Name));
                dbContext.Users.Add(testUser);
                dbContext.SaveChanges();
                if (position != null)
                {
                    testUser.AssignPosition(new UserPosition(testUser.Id, position.Id, position.Name));
                    dbContext.SaveChanges();
                }
            }

            // 初始化示例用户：张三（市场部-市场经理）、李四（人力资源部-HR专员）
            if (!dbContext.Users.Any(u => u.Name == "zhangsan"))
            {
                var mktDept = dbContext.Depts.FirstOrDefault(r => r.Name == "市场部");
                var hrDept = dbContext.Depts.FirstOrDefault(r => r.Name == "人力资源部");
                var userRole = dbContext.Roles.FirstOrDefault(r => r.Name == "普通用户");
                var mmPos = dbContext.Positions.FirstOrDefault(p => p.Code == "MM");
                var hrsPos = dbContext.Positions.FirstOrDefault(p => p.Code == "HRS");
                if (mktDept != null && hrDept != null && userRole != null)
                {
                    var zhangsan = new User(
                    "zhangsan",
                    "13800138002",
                    passwordHasher.Hash("123456"),
                    new List<UserRole> { new UserRole(userRole.Id, userRole.Name) },
                    "张三",
                    1,
                    "zhangsan@example.com",
                    "男",
                    DateTimeOffset.UtcNow.AddYears(-28),
                    new UserId(0),
                    "",
                    "",
                    "本科",
                    "",
                    ""
                );
                zhangsan.AssignDept(new UserDept(zhangsan.Id, mktDept.Id, mktDept.Name));
                dbContext.Users.Add(zhangsan);
                dbContext.SaveChanges();
                if (mmPos != null)
                {
                    zhangsan.AssignPosition(new UserPosition(zhangsan.Id, mmPos.Id, mmPos.Name));
                    dbContext.SaveChanges();
                }

                var lisi = new User(
                    "lisi",
                    "13800138003",
                    passwordHasher.Hash("123456"),
                    new List<UserRole> { new UserRole(userRole.Id, userRole.Name) },
                    "李四",
                    1,
                    "lisi@example.com",
                    "女",
                    DateTimeOffset.UtcNow.AddYears(-26),
                    new UserId(0),
                    "",
                    "",
                    "本科",
                    "",
                    ""
                );
                lisi.AssignDept(new UserDept(lisi.Id, hrDept.Id, hrDept.Name));
                dbContext.Users.Add(lisi);
                dbContext.SaveChanges();
                if (hrsPos != null)
                {
                    lisi.AssignPosition(new UserPosition(lisi.Id, hrsPos.Id, hrsPos.Name));
                    dbContext.SaveChanges();
                }
                }
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


