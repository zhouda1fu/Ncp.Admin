using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record IncomeExpenseTypeOptionDto(IncomeExpenseTypeOptionId Id, string Name, int TypeValue, int SortOrder);

public class IncomeExpenseTypeOptionQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<IReadOnlyList<IncomeExpenseTypeOptionDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.IncomeExpenseTypeOptions
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.TypeValue)
            .Select(x => new IncomeExpenseTypeOptionDto(x.Id, x.Name, x.TypeValue, x.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
