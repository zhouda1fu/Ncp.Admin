using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Services;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

namespace Ncp.Admin.Infrastructure;

public partial class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IMediator mediator,
    IDataPermissionProvider? dataPermissionProvider = null)
    : AppDbContextBase(options, mediator)
    , IMySqlCapDataStorage
{
    /// <summary>
    /// 数据权限提供者；为 null 时不应用数据权限过滤。
    /// </summary>
    private readonly IDataPermissionProvider? _dataPermissionProvider = dataPermissionProvider;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        if (_dataPermissionProvider != null)
        {
            modelBuilder.Entity<WorkflowInstance>().HasQueryFilter(wi =>
                _dataPermissionProvider.Scope == DataScope.All
                || (_dataPermissionProvider.Scope == DataScope.Self && wi.InitiatorId == _dataPermissionProvider.UserId)
                || (_dataPermissionProvider.Scope == DataScope.Dept && wi.InitiatorDeptId == _dataPermissionProvider.DeptId)
                || (_dataPermissionProvider.Scope == DataScope.DeptAndSub && _dataPermissionProvider.AuthorizedDeptIds.Contains(wi.InitiatorDeptId)));
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
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowNode> WorkflowNodes => Set<WorkflowNode>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Notification> Notifications => Set<Notification>();
}
