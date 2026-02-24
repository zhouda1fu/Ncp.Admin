using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
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
    , IMySqlCapDataStorage
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

        if (_contextAccessor != null)
        {
            modelBuilder.Entity<WorkflowInstance>().HasQueryFilter(wi =>
                _contextAccessor.GetContext<DataPermissionContext>() == null
                || _contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.All
                || (_contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.Self && wi.InitiatorId == _contextAccessor.GetContext<DataPermissionContext>()!.UserId)
                || (_contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.Dept && wi.InitiatorDeptId == _contextAccessor.GetContext<DataPermissionContext>()!.DeptId)
                || (_contextAccessor.GetContext<DataPermissionContext>()!.Scope == DataScope.DeptAndSub && _contextAccessor.GetContext<DataPermissionContext>()!.AuthorizedDeptIds.Contains(wi.InitiatorDeptId)));
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
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
}
