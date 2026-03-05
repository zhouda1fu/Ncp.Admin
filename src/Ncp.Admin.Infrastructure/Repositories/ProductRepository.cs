using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 产品仓储接口
/// </summary>
public interface IProductRepository : IRepository<Product, ProductId>
{
    Task RemoveAsync(ProductId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 产品仓储实现
/// </summary>
public class ProductRepository(ApplicationDbContext context)
    : RepositoryBase<Product, ProductId, ApplicationDbContext>(context), IProductRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Products.FindAsync([id], cancellationToken);
        if (entity != null)
            context.Products.Remove(entity);
    }
}
