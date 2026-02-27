using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 客户来源仓储接口
/// </summary>
public interface ICustomerSourceRepository : IRepository<CustomerSource, CustomerSourceId> { }

/// <summary>
/// 客户来源仓储实现
/// </summary>
public class CustomerSourceRepository(ApplicationDbContext context)
    : RepositoryBase<CustomerSource, CustomerSourceId, ApplicationDbContext>(context), ICustomerSourceRepository { }
