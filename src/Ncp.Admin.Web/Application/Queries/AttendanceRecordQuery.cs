using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 考勤记录查询 DTO
/// </summary>
public record AttendanceRecordQueryDto(
    AttendanceRecordId Id,
    UserId UserId,
    DateTimeOffset CheckInAt,
    DateTimeOffset? CheckOutAt,
    AttendanceSource Source,
    string? Location,
    DateTimeOffset CreatedAt);

/// <summary>
/// 考勤记录分页查询入参
/// </summary>
public class AttendanceRecordQueryInput : PageRequest
{
    /// <summary>
    /// 用户ID筛选
    /// </summary>
    public UserId? UserId { get; set; }
    /// <summary>
    /// 签到日期起
    /// </summary>
    public DateOnly? DateFrom { get; set; }
    /// <summary>
    /// 签到日期止
    /// </summary>
    public DateOnly? DateTo { get; set; }
}

/// <summary>
/// 考勤记录查询服务
/// </summary>
public class AttendanceRecordQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询考勤记录
    /// </summary>
    public async Task<AttendanceRecordQueryDto?> GetByIdAsync(AttendanceRecordId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new AttendanceRecordQueryDto(r.Id, r.UserId, r.CheckInAt, r.CheckOutAt, r.Source, r.Location, r.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询考勤记录
    /// </summary>
    public async Task<PagedData<AttendanceRecordQueryDto>> GetPagedAsync(AttendanceRecordQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AttendanceRecords.AsNoTracking();
        if (input.UserId != null)
            query = query.Where(r => r.UserId == input.UserId);
        if (input.DateFrom.HasValue)
        {
            var start = input.DateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(r => r.CheckInAt >= start);
        }
        if (input.DateTo.HasValue)
        {
            var end = input.DateTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc).AddMilliseconds(1);
            query = query.Where(r => r.CheckInAt < end);
        }

        return await query
            .OrderByDescending(r => r.CheckInAt)
            .Select(r => new AttendanceRecordQueryDto(r.Id, r.UserId, r.CheckInAt, r.CheckOutAt, r.Source, r.Location, r.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
