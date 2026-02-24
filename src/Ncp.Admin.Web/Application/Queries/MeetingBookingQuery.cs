using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 会议室预订查询 DTO（含会议室名称）
/// </summary>
public record MeetingBookingQueryDto(
    MeetingBookingId Id,
    MeetingRoomId MeetingRoomId,
    string MeetingRoomName,
    UserId BookerId,
    string Title,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    MeetingBookingStatus Status,
    DateTimeOffset CreatedAt);

/// <summary>
/// 会议室预订分页查询入参
/// </summary>
public class MeetingBookingQueryInput : PageRequest
{
    /// <summary>
    /// 会议室ID筛选
    /// </summary>
    public MeetingRoomId? MeetingRoomId { get; set; }
    /// <summary>
    /// 预订人ID筛选
    /// </summary>
    public UserId? BookerId { get; set; }
    /// <summary>
    /// 开始时间起
    /// </summary>
    public DateTimeOffset? StartFrom { get; set; }
    /// <summary>
    /// 开始时间止
    /// </summary>
    public DateTimeOffset? StartTo { get; set; }
}

/// <summary>
/// 会议室预订查询服务（关联会议室名称）
/// </summary>
public class MeetingBookingQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 分页查询会议室预订
    /// </summary>
    public async Task<PagedData<MeetingBookingQueryDto>> GetPagedAsync(MeetingBookingQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.MeetingBookings
            .AsNoTracking()
            .Join(dbContext.MeetingRooms, b => b.MeetingRoomId, r => r.Id, (b, r) => new { b, r });
        if (input.MeetingRoomId != null)
            query = query.Where(x => x.b.MeetingRoomId == input.MeetingRoomId);
        if (input.BookerId != null)
            query = query.Where(x => x.b.BookerId == input.BookerId);
        if (input.StartFrom.HasValue)
            query = query.Where(x => x.b.StartAt >= input.StartFrom.Value);
        if (input.StartTo.HasValue)
            query = query.Where(x => x.b.StartAt <= input.StartTo.Value);

        return await query
            .OrderBy(x => x.b.StartAt)
            .Select(x => new MeetingBookingQueryDto(
                x.b.Id,
                x.b.MeetingRoomId,
                x.r.Name,
                x.b.BookerId,
                x.b.Title,
                x.b.StartAt,
                x.b.EndAt,
                x.b.Status,
                x.b.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
