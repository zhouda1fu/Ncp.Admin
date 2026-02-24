using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 会议室预订仓储接口
/// </summary>
public interface IMeetingBookingRepository : IRepository<MeetingBooking, MeetingBookingId>
{
    /// <summary>
    /// 检查指定会议室在给定时段内是否已有有效预订（可用于排除某条记录，如编辑时）
    /// </summary>
    /// <param name="roomId">会议室ID</param>
    /// <param name="start">时段开始</param>
    /// <param name="end">时段结束</param>
    /// <param name="excludeId">要排除的预订ID（如编辑时排除自身）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存在冲突返回 true</returns>
    Task<bool> HasConflictAsync(MeetingRoomId roomId, DateTimeOffset start, DateTimeOffset end, MeetingBookingId? excludeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 会议室预订仓储实现
/// </summary>
public class MeetingBookingRepository(ApplicationDbContext context)
    : RepositoryBase<MeetingBooking, MeetingBookingId, ApplicationDbContext>(context), IMeetingBookingRepository
{
    /// <inheritdoc />
    public async Task<bool> HasConflictAsync(MeetingRoomId roomId, DateTimeOffset start, DateTimeOffset end, MeetingBookingId? excludeId, CancellationToken cancellationToken = default)
    {
        var query = DbContext.MeetingBookings
            .Where(b => b.MeetingRoomId == roomId && b.Status == MeetingBookingStatus.Booked
                && b.StartAt < end && b.EndAt > start);
        if (excludeId != null)
            query = query.Where(b => b.Id != excludeId);
        return await query.AnyAsync(cancellationToken);
    }
}
