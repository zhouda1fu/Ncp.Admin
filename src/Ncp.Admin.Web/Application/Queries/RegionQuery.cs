using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record RegionDto(RegionId Id, string Name, RegionId ParentId, int Level, int SortOrder);

public class RegionQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<RegionDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Regions
            .AsNoTracking()
            .OrderBy(x => x.Level)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new RegionDto(x.Id, x.Name, x.ParentId, x.Level, x.SortOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RegionDto>> GetChildrenAsync(long parentCode, CancellationToken cancellationToken = default)
    {
        var parentId = new RegionId(parentCode);
        return await dbContext.Regions
            .AsNoTracking()
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new RegionDto(x.Id, x.Name, x.ParentId, x.Level, x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
