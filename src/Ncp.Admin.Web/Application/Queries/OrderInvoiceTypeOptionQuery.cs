using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record OrderInvoiceTypeOptionDto(
    OrderInvoiceTypeOptionId Id,
    string Name,
    int TypeValue,
    int SortOrder);

public class OrderInvoiceTypeOptionQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<OrderInvoiceTypeOptionDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderInvoiceTypeOptions
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.TypeValue)
            .Select(x => new OrderInvoiceTypeOptionDto(
                x.Id,
                x.Name,
                x.TypeValue,
                x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
