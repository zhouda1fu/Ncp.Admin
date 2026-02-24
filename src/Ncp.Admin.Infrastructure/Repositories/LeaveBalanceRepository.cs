using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 请假余额仓储接口
/// </summary>
public interface ILeaveBalanceRepository : IRepository<LeaveBalance, LeaveBalanceId>
{
    /// <summary>
    /// 按用户、年度、类型获取余额
    /// </summary>
    Task<LeaveBalance?> GetByUserYearTypeAsync(UserId userId, int year, LeaveType leaveType, CancellationToken cancellationToken = default);
}

/// <summary>
/// 请假余额仓储实现
/// </summary>
public class LeaveBalanceRepository(ApplicationDbContext context)
    : RepositoryBase<LeaveBalance, LeaveBalanceId, ApplicationDbContext>(context), ILeaveBalanceRepository
{
    public async Task<LeaveBalance?> GetByUserYearTypeAsync(UserId userId, int year, LeaveType leaveType, CancellationToken cancellationToken = default)
    {
        return await DbContext.LeaveBalances
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Year == year && b.LeaveType == leaveType, cancellationToken);
    }
}
