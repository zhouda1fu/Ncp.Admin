using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Repository;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IOrderRepository : IRepository<Order, OrderId>
{
}

public class OrderRepository(ApplicationDbContext context) : RepositoryBase<Order, OrderId, ApplicationDbContext>(context), IOrderRepository
{
}

