using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsMethodAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 订单物流方式仓储接口
/// </summary>
public interface IOrderLogisticsMethodRepository : IRepository<OrderLogisticsMethod, OrderLogisticsMethodId>
{
}

/// <summary>
/// 订单物流方式仓储实现
/// </summary>
public class OrderLogisticsMethodRepository(ApplicationDbContext context)
    : RepositoryBase<OrderLogisticsMethod, OrderLogisticsMethodId, ApplicationDbContext>(context), IOrderLogisticsMethodRepository
{
}
