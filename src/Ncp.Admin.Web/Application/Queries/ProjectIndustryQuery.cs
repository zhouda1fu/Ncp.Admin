using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record ProjectIndustryDto(ProjectIndustryId Id, string Name, int SortOrder);

public class ProjectIndustryQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<ProjectIndustryDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectIndustries
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ProjectIndustryDto(x.Id, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
