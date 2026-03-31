using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 项目仓储接口
/// </summary>
public interface IProjectRepository : IRepository<Project, ProjectId>
{
    /// <summary>
    /// 删除项目
    /// </summary>
    Task RemoveAsync(ProjectId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目仓储实现
/// </summary>
public class ProjectRepository(ApplicationDbContext context)
    : RepositoryBase<Project, ProjectId, ApplicationDbContext>(context), IProjectRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Projects.FindAsync([id], cancellationToken);
        if (entity != null)
        {
            context.Projects.Remove(entity);
        }
    }
}
