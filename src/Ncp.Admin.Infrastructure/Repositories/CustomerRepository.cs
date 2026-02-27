using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 客户仓储接口
/// </summary>
public interface ICustomerRepository : IRepository<Customer, CustomerId>
{
    /// <summary>
    /// 移除客户（仅公海客户可删除时由调用方校验）
    /// </summary>
    Task RemoveAsync(CustomerId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 客户仓储实现
/// </summary>
public class CustomerRepository(ApplicationDbContext context)
    : RepositoryBase<Customer, CustomerId, ApplicationDbContext>(context), ICustomerRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Customers.FindAsync([id], cancellationToken);
        if (entity != null)
            context.Customers.Remove(entity);
    }
}
