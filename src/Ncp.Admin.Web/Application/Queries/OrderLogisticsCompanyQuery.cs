using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsCompanyAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record OrderLogisticsCompanyDto(
    OrderLogisticsCompanyId Id,
    string Name,
    int TypeValue,
    int Sort);

public class OrderLogisticsCompanyQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<OrderLogisticsCompanyDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.OrderLogisticsCompanies
            .AsNoTracking()
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(x => new OrderLogisticsCompanyDto(
                x.Id,
                x.Name,
                x.TypeValue,
                x.Sort))
            .ToListAsync(cancellationToken);
    }
}
