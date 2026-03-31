using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptions;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 合同类型选项仓储接口
/// </summary>
public interface IContractTypeOptionRepository : IRepository<ContractTypeOption, ContractTypeOptionId>
{
    Task RemoveAsync(ContractTypeOptionId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 合同类型选项仓储实现
/// </summary>
public class ContractTypeOptionRepository(ApplicationDbContext context)
    : RepositoryBase<ContractTypeOption, ContractTypeOptionId, ApplicationDbContext>(context), IContractTypeOptionRepository
{
    /// <inheritdoc />
    public async Task RemoveAsync(ContractTypeOptionId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.ContractTypeOptions.FindAsync([id], cancellationToken);
        if (entity != null)
            context.ContractTypeOptions.Remove(entity);
    }
}
