using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record CustomerSourceDto(CustomerSourceId Id, string Name, int SortOrder);

public class CustomerSourceQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<CustomerSourceDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CustomerSources
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CustomerSourceDto(x.Id, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
