using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 行业仓储接口
/// </summary>
public interface IIndustryRepository : IRepository<Industry, IndustryId>
{
    /// <summary>
    /// 移除行业
    /// </summary>
    Task RemoveAsync(IndustryId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 行业仓储实现
/// </summary>
public class IndustryRepository(ApplicationDbContext context)
    : RepositoryBase<Industry, IndustryId, ApplicationDbContext>(context), IIndustryRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(IndustryId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Industries.FindAsync([id], cancellationToken);
        if (entity != null)
            context.Industries.Remove(entity);
    }
}
