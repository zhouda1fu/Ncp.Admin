using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 订单仓储接口
/// </summary>
public interface IOrderRepository : IRepository<Order, OrderId>
{
}

/// <summary>
/// 订单仓储实现
/// </summary>
public class OrderRepository(ApplicationDbContext context)
    : RepositoryBase<Order, OrderId, ApplicationDbContext>(context), IOrderRepository
{
}
