using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 项目行业仓储接口
/// </summary>
public interface IProjectIndustryRepository : IRepository<ProjectIndustry, ProjectIndustryId>
{
    Task RemoveAsync(ProjectIndustryId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目行业仓储实现
/// </summary>
public class ProjectIndustryRepository(ApplicationDbContext context)
    : RepositoryBase<ProjectIndustry, ProjectIndustryId, ApplicationDbContext>(context), IProjectIndustryRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProjectIndustryId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.ProjectIndustries.FindAsync([id], cancellationToken);
        if (entity != null)
            context.ProjectIndustries.Remove(entity);
    }
}
