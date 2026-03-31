using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 供应商仓储
/// </summary>
public interface ISupplierRepository : IRepository<Supplier, SupplierId>
{
    Task RemoveAsync(SupplierId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 供应商仓储实现
/// </summary>
public class SupplierRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Supplier, SupplierId, ApplicationDbContext>(dbContext), ISupplierRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(SupplierId id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Suppliers.FindAsync([id], cancellationToken);
        if (entity != null)
            dbContext.Suppliers.Remove(entity);
    }
}
