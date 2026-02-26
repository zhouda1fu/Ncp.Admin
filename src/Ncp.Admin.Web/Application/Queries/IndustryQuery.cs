using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record IndustryDto(IndustryId Id, string Name, IndustryId? ParentId, int SortOrder, string? Remark);

public class IndustryQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<IndustryDto>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        var list = await dbContext.Industries
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new IndustryDto(x.Id, x.Name, x.ParentId, x.SortOrder, x.Remark))
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task<IReadOnlyList<IndustryDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await GetTreeAsync(cancellationToken);
    }
}
