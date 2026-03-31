using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record OrderLogisticsMethodDto(
    OrderLogisticsMethodId Id,
    string Name,
    int TypeValue,
    int Sort);

public class OrderLogisticsMethodQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<OrderLogisticsMethodDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderLogisticsMethods
            .AsNoTracking()
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(x => new OrderLogisticsMethodDto(
                x.Id,
                x.Name,
                x.TypeValue,
                x.Sort))
            .ToListAsync(cancellationToken);
    }
}
