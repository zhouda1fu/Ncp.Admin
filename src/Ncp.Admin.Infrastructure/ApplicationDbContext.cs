using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptions;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Services;
using NetCorePal.Context;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

namespace Ncp.Admin.Infrastructure;

public partial class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IMediator mediator,
    IContextAccessor? contextAccessor = null)
    : AppDbContextBase(options, mediator)
    , IPostgreSqlCapDataStorage
{
    /// <summary>
    /// 上下文访问器（NetCorePal）；为 null 时不应用数据权限过滤。
    /// </summary>
    private readonly IContextAccessor? _contextAccessor = contextAccessor;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        // 全局：所有 DateTimeOffset 写入 PostgreSQL 时转为 UTC（Npgsql 仅接受 Offset=0）
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTimeOffset, DateTimeOffset>(
                            v => v.ToUniversalTime(),
                            v => v));
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
                            v => v.HasValue ? v.Value.ToUniversalTime() : null,
                            v => v));
                }
            }
        }

        if (_contextAccessor != null)
        {
            modelBuilder.Entity<WorkflowInstance>().HasQueryFilter(wi =>
                _contextAccessor.GetContext<DataPermissionContext>() == null
                || _contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.All
                || (_contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.Self && wi.InitiatorId == _contextAccessor.GetContext<DataPermissionContext>()!.UserId)
                || (_contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.Dept && wi.InitiatorDeptId == _contextAccessor.GetContext<DataPermissionContext>()!.DeptId)
                || ((_contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.DeptAndSub
                     || _contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.CustomDeptAndSub)
                    && _contextAccessor.GetContext<DataPermissionContext>()!.AuthorizedDeptIds.Contains(wi.InitiatorDeptId)));
        }
    }



    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ConfigureStronglyTypedIdValueConverter(configurationBuilder);
        base.ConfigureConventions(configurationBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Dept> Depts => Set<Dept>();
    public DbSet<UserDept> UserDepts => Set<UserDept>();
    public DbSet<UserPosition> UserPositions => Set<UserPosition>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementReadRecord> AnnouncementReadRecords => Set<AnnouncementReadRecord>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ExpenseClaim> ExpenseClaims => Set<ExpenseClaim>();
    public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();
    public DbSet<MeetingBooking> MeetingBookings => Set<MeetingBooking>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<ProjectTaskComment> ProjectTaskComments => Set<ProjectTaskComment>();
    public DbSet<ContactGroup> ContactGroups => Set<ContactGroup>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();
    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
    public DbSet<ChatGroupMember> ChatGroupMembers => Set<ChatGroupMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetAllocation> AssetAllocations => Set<AssetAllocation>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleBooking> VehicleBookings => Set<VehicleBooking>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderCategory> OrderCategories => Set<OrderCategory>();
    public DbSet<OrderRemark> OrderRemarks => Set<OrderRemark>();
    public DbSet<OrderLogisticsCompany> OrderLogisticsCompanies => Set<OrderLogisticsCompany>();
    public DbSet<OrderLogisticsMethod> OrderLogisticsMethods => Set<OrderLogisticsMethod>();
    public DbSet<OrderPush> OrderPushes => Set<OrderPush>();
    public DbSet<OrderInvoiceTypeOption> OrderInvoiceTypeOptions => Set<OrderInvoiceTypeOption>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<ProductParameter> ProductParameters => Set<ProductParameter>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<CustomerSource> CustomerSources => Set<CustomerSource>();
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<ProjectType> ProjectTypes => Set<ProjectType>();
    public DbSet<ProjectStatusOption> ProjectStatusOptions => Set<ProjectStatusOption>();
    public DbSet<ProjectIndustry> ProjectIndustries => Set<ProjectIndustry>();
    public DbSet<ContractTypeOption> ContractTypeOptions => Set<ContractTypeOption>();
    public DbSet<IncomeExpenseTypeOption> IncomeExpenseTypeOptions => Set<IncomeExpenseTypeOption>();
    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();
}
