using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
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

        ApplyGlobalDateTimeOffsetValueConversions(modelBuilder);

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
    public DbSet<DeptResponsibleUser> DeptResponsibleUsers => Set<DeptResponsibleUser>();
    public DbSet<UserDept> UserDepts => Set<UserDept>();
    public DbSet<UserPosition> UserPositions => Set<UserPosition>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowDefinitionVersion> WorkflowDefinitionVersions => Set<WorkflowDefinitionVersion>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<WorkflowTaskAssignmentSnapshot> WorkflowTaskAssignmentSnapshots => Set<WorkflowTaskAssignmentSnapshot>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserHomeDashboardPreference> UserHomeDashboardPreferences => Set<UserHomeDashboardPreference>();
    public DbSet<UserCalendarMemo> UserCalendarMemos => Set<UserCalendarMemo>();
    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();

    /// <summary>
    /// 领域模型 <see cref="DateTimeOffset"/> 与 PostgreSQL <c>timestamp</c>（UTC <see cref="DateTime"/>）互转。
    /// 写入时统一为 UTC；读取时兼容 <c>timestamp without time zone</c> 等返回 <see cref="DateTime"/> 的列。
    /// </summary>
    private static readonly ValueConverter<DateTimeOffset, DateTime> UtcDateTimeOffsetConverter = new(
        model => model.ToUniversalTime().UtcDateTime,
        provider => FromUtcDateTime(provider));

    private static readonly ValueConverter<DateTimeOffset?, DateTime?> UtcNullableDateTimeOffsetConverter = new(
        model => model.HasValue ? model.Value.ToUniversalTime().UtcDateTime : null,
        provider => provider.HasValue ? FromUtcDateTime(provider.Value) : null);

    private static void ApplyGlobalDateTimeOffsetValueConversions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.GetValueConverter() is not null)
                    continue;

                if (property.ClrType == typeof(DateTimeOffset))
                    property.SetValueConverter(UtcDateTimeOffsetConverter);
                else if (property.ClrType == typeof(DateTimeOffset?))
                    property.SetValueConverter(UtcNullableDateTimeOffsetConverter);
            }
        }
    }

    private static DateTimeOffset FromUtcDateTime(DateTime provider)
    {
        switch (provider.Kind)
        {
            case DateTimeKind.Utc:
                return new DateTimeOffset(provider);
            case DateTimeKind.Local:
                return provider.ToUniversalTime();
            default:
                return new DateTimeOffset(DateTime.SpecifyKind(provider, DateTimeKind.Utc));
        }
    }
}
