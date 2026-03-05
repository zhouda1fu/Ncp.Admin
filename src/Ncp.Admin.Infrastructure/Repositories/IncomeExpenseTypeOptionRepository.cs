using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 收支类型选项仓储接口
/// </summary>
public interface IIncomeExpenseTypeOptionRepository : IRepository<IncomeExpenseTypeOption, IncomeExpenseTypeOptionId>
{
    Task RemoveAsync(IncomeExpenseTypeOptionId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 收支类型选项仓储实现
/// </summary>
public class IncomeExpenseTypeOptionRepository(ApplicationDbContext context)
    : RepositoryBase<IncomeExpenseTypeOption, IncomeExpenseTypeOptionId, ApplicationDbContext>(context), IIncomeExpenseTypeOptionRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(IncomeExpenseTypeOptionId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.IncomeExpenseTypeOptions.FindAsync([id], cancellationToken);
        if (entity != null)
            context.IncomeExpenseTypeOptions.Remove(entity);
    }
}
