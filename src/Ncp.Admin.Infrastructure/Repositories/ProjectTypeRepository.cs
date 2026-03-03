using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 项目类型仓储接口
/// </summary>
public interface IProjectTypeRepository : IRepository<ProjectType, ProjectTypeId>
{
    Task RemoveAsync(ProjectTypeId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目类型仓储实现
/// </summary>
public class ProjectTypeRepository(ApplicationDbContext context)
    : RepositoryBase<ProjectType, ProjectTypeId, ApplicationDbContext>(context), IProjectTypeRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProjectTypeId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.ProjectTypes.FindAsync([id], cancellationToken);
        if (entity != null)
            context.ProjectTypes.Remove(entity);
    }
}
