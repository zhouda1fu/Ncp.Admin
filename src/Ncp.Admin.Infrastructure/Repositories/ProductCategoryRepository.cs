using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 产品分类仓储
/// </summary>
public interface IProductCategoryRepository : IRepository<ProductCategory, ProductCategoryId>
{
    Task RemoveAsync(ProductCategoryId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 产品分类仓储实现
/// </summary>
public class ProductCategoryRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ProductCategory, ProductCategoryId, ApplicationDbContext>(dbContext), IProductCategoryRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProductCategoryId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ProductCategories.FindAsync([id], cancellationToken);
        if (entity != null)
            dbContext.ProductCategories.Remove(entity);
    }
}
