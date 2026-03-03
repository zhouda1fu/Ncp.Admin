using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record ProjectTypeDto(ProjectTypeId Id, string Name, int SortOrder);

public class ProjectTypeQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<ProjectTypeDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectTypes
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ProjectTypeDto(x.Id, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
