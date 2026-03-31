using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 产品参数仓储
/// </summary>
public interface IProductParameterRepository : IRepository<ProductParameter, ProductParameterId>
{
    Task RemoveAsync(ProductParameterId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 产品参数仓储实现
/// </summary>
public class ProductParameterRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ProductParameter, ProductParameterId, ApplicationDbContext>(dbContext), IProductParameterRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ProductParameterId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ProductParameters.FindAsync([id], cancellationToken);
        if (entity != null)
            dbContext.ProductParameters.Remove(entity);
    }
}
