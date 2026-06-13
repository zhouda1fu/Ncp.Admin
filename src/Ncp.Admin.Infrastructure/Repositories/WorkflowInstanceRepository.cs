using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Microsoft.EntityFrameworkCore;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 流程实例仓储接口
/// </summary>
public interface IWorkflowInstanceRepository : IRepository<WorkflowInstance, WorkflowInstanceId>
{
    /// <summary>
    /// 获取流程实例并加载任务集合。
    /// </summary>
    Task<WorkflowInstance?> GetWithTasksIgnoringQueryFiltersAsync(
        WorkflowInstanceId id,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 流程实例仓储实现
/// </summary>
public class WorkflowInstanceRepository(ApplicationDbContext context)
    : RepositoryBase<WorkflowInstance, WorkflowInstanceId, ApplicationDbContext>(context), IWorkflowInstanceRepository
{
    public Task<WorkflowInstance?> GetWithTasksIgnoringQueryFiltersAsync(
        WorkflowInstanceId id,
        CancellationToken cancellationToken = default)
    {
        return context.WorkflowInstances
            .IgnoreQueryFilters()
            .Include(i => i.Tasks)
                .ThenInclude(t => t.AssignmentSnapshots)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
}
