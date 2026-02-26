using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 合同仓储接口
/// </summary>
public interface IContractRepository : IRepository<Contract, ContractId> { }

/// <summary>
/// 合同仓储实现
/// </summary>
public class ContractRepository(ApplicationDbContext context)
    : RepositoryBase<Contract, ContractId, ApplicationDbContext>(context), IContractRepository { }
