using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 产品类型仓储
/// </summary>
public interface IProductTypeRepository : IRepository<ProductType, ProductTypeId>
{
    Task RemoveAsync(ProductTypeId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 产品类型仓储实现
/// </summary>
public class ProductTypeRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ProductType, ProductTypeId, ApplicationDbContext>(dbContext), IProductTypeRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProductTypeId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ProductTypes.FindAsync([id], cancellationToken);
        if (entity != null)
            dbContext.ProductTypes.Remove(entity);
    }
}
