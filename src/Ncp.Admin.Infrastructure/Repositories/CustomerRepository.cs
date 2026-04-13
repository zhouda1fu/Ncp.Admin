using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 客户仓储接口
/// </summary>
public interface ICustomerRepository : IRepository<Customer, CustomerId>
{
    /// <summary>
    /// 按 ID 加载客户并包含联系人（修改联系人、校验联系记录关联等必须调用，勿依赖全局 AutoInclude）。
    /// </summary>
    Task<Customer?> GetWithContactsAsync(CustomerId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按 ID 加载客户并包含行业关联（更新客户档案同步行业列表时必须调用）。
    /// </summary>
    Task<Customer?> GetWithIndustriesAsync(CustomerId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按 ID 加载客户并包含共享关系（共享/取消共享时必须调用）。
    /// </summary>
    Task<Customer?> GetWithSharesAsync(CustomerId id, CancellationToken cancellationToken = default);

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
    public async Task<Customer?> GetWithContactsAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Customers
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Customer?> GetWithIndustriesAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Customers
            .Include(c => c.Industries)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Customer?> GetWithSharesAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Customers
            .Include(c => c.Shares)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        var entity = await DbContext.Customers.FindAsync([id], cancellationToken);
        if (entity != null)
            DbContext.Customers.Remove(entity);
    }
}
