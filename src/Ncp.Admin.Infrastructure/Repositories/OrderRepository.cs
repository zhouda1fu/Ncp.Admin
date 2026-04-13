using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 订单仓储接口
/// </summary>
public interface IOrderRepository : IRepository<Order, OrderId>
{
    /// <summary>
    /// 加载订单聚合用于编辑（含明细与按分类合同优惠）
    /// </summary>
    Task<Order?> GetAggregateForEditAsync(OrderId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 订单仓储实现
/// </summary>
public class OrderRepository(ApplicationDbContext context)
    : RepositoryBase<Order, OrderId, ApplicationDbContext>(context), IOrderRepository
{
    public Task<Order?> GetAggregateForEditAsync(OrderId id, CancellationToken cancellationToken = default) =>
        DbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Categories)
            .Include(o => o.Remarks)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
}
