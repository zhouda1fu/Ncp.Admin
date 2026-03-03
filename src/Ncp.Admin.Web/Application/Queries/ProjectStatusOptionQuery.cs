using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record ProjectStatusOptionDto(ProjectStatusOptionId Id, string Name, string? Code, int SortOrder);

public class ProjectStatusOptionQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<ProjectStatusOptionDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectStatusOptions
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ProjectStatusOptionDto(x.Id, x.Name, x.Code, x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
