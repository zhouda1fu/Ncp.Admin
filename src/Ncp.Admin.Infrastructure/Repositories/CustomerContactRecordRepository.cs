using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerContactRecordAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 客户联系记录仓储
/// </summary>
public interface ICustomerContactRecordRepository : IRepository<CustomerContactRecord, CustomerContactRecordId>
{
    /// <summary>
    /// 加载记录及其与联系人的关联行（修改联系记录时必须 Include，否则 <see cref="CustomerContactRecord.Contacts"/> 在内存中为空会触发主键冲突）。
    /// </summary>
    Task<CustomerContactRecord?> GetWithContactLinksAsync(
        CustomerContactRecordId id,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 客户联系记录仓储实现
/// </summary>
public class CustomerContactRecordRepository(ApplicationDbContext context)
    : RepositoryBase<CustomerContactRecord, CustomerContactRecordId, ApplicationDbContext>(context),
        ICustomerContactRecordRepository
{
    /// <inheritdoc />
    public async Task<CustomerContactRecord?> GetWithContactLinksAsync(
        CustomerContactRecordId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.CustomerContactRecords
            .Include(r => r.Contacts)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
