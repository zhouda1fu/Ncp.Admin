using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 请假申请仓储接口
/// </summary>
public interface ILeaveRequestRepository : IRepository<LeaveRequest, LeaveRequestId> { }

/// <summary>
/// 请假申请仓储实现
/// </summary>
public class LeaveRequestRepository(ApplicationDbContext context)
    : RepositoryBase<LeaveRequest, LeaveRequestId, ApplicationDbContext>(context), ILeaveRequestRepository { }
