using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 项目任务仓储接口
/// </summary>
public interface IProjectTaskRepository : IRepository<ProjectTask, ProjectTaskId>
{
    /// <summary>
    /// 删除项目任务
    /// </summary>
    System.Threading.Tasks.Task RemoveAsync(ProjectTaskId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目任务仓储实现
/// </summary>
public class ProjectTaskRepository(ApplicationDbContext context)
    : RepositoryBase<ProjectTask, ProjectTaskId, ApplicationDbContext>(context), IProjectTaskRepository
{
    /// <inheritdoc />
    public async System.Threading.Tasks.Task RemoveAsync(ProjectTaskId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.ProjectTasks.FindAsync([id], cancellationToken);
        if (entity != null)
            context.ProjectTasks.Remove(entity);
    }
}
