using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 考勤记录仓储接口
/// </summary>
public interface IAttendanceRecordRepository : IRepository<AttendanceRecord, AttendanceRecordId>
{
    /// <summary>
    /// 按用户与日期查询当日考勤记录（用于防重复打卡）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="date">日期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>当日考勤记录，无则 null</returns>
    Task<AttendanceRecord?> GetTodayByUserAsync(UserId userId, DateOnly date, CancellationToken cancellationToken = default);
}

/// <summary>
/// 考勤记录仓储实现
/// </summary>
public class AttendanceRecordRepository(ApplicationDbContext context)
    : RepositoryBase<AttendanceRecord, AttendanceRecordId, ApplicationDbContext>(context), IAttendanceRecordRepository
{
    /// <inheritdoc />
    public async Task<AttendanceRecord?> GetTodayByUserAsync(UserId userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc).AddMilliseconds(1);
        return await DbContext.AttendanceRecords
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CheckInAt >= start && r.CheckInAt < end, cancellationToken);
    }
}
