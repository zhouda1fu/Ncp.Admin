using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 报销单仓储接口
/// </summary>
public interface IExpenseClaimRepository : IRepository<ExpenseClaim, ExpenseClaimId> { }

/// <summary>
/// 报销单仓储实现
/// </summary>
public class ExpenseClaimRepository(ApplicationDbContext context)
    : RepositoryBase<ExpenseClaim, ExpenseClaimId, ApplicationDbContext>(context), IExpenseClaimRepository { }
