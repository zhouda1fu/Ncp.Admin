using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 项目状态选项仓储接口
/// </summary>
public interface IProjectStatusOptionRepository : IRepository<ProjectStatusOption, ProjectStatusOptionId>
{
    Task RemoveAsync(ProjectStatusOptionId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目状态选项仓储实现
/// </summary>
public class ProjectStatusOptionRepository(ApplicationDbContext context)
    : RepositoryBase<ProjectStatusOption, ProjectStatusOptionId, ApplicationDbContext>(context), IProjectStatusOptionRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProjectStatusOptionId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.ProjectStatusOptions.FindAsync([id], cancellationToken);
        if (entity != null)
            context.ProjectStatusOptions.Remove(entity);
    }
}
