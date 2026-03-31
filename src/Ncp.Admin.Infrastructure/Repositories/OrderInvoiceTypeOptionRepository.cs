using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IOrderInvoiceTypeOptionRepository : IRepository<OrderInvoiceTypeOption, OrderInvoiceTypeOptionId>
{
}

public class OrderInvoiceTypeOptionRepository(ApplicationDbContext context)
    : RepositoryBase<OrderInvoiceTypeOption, OrderInvoiceTypeOptionId, ApplicationDbContext>(context), IOrderInvoiceTypeOptionRepository
{
}
