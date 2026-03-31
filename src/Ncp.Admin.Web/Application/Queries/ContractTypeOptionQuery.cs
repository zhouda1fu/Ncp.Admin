using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptions;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record ContractTypeOptionDto(
    ContractTypeOptionId Id,
    string Name,
    int TypeValue,
    bool OrderSigningCompanyOptionDisplay,
    int SortOrder);

public class ContractTypeOptionQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<ContractTypeOptionDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ContractTypeOptions
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.TypeValue)
            .Select(x => new ContractTypeOptionDto(
                x.Id,
                x.Name,
                x.TypeValue,
                x.OrderSigningCompanyOptionDisplay,
                x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
