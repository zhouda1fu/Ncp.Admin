using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 排班仓储接口
/// </summary>
public interface IScheduleRepository : IRepository<Schedule, ScheduleId> { }

/// <summary>
/// 排班仓储实现
/// </summary>
public class ScheduleRepository(ApplicationDbContext context)
    : RepositoryBase<Schedule, ScheduleId, ApplicationDbContext>(context), IScheduleRepository { }
