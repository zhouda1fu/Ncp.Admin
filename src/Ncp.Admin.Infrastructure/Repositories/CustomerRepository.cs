using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 客户仓储接口
/// </summary>
public interface ICustomerRepository : IRepository<Customer, CustomerId> { }

/// <summary>
/// 客户仓储实现
/// </summary>
public class CustomerRepository(ApplicationDbContext context)
    : RepositoryBase<Customer, CustomerId, ApplicationDbContext>(context), ICustomerRepository { }
