using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 排班查询 DTO
/// </summary>
public record ScheduleQueryDto(
    ScheduleId Id,
    UserId UserId,
    DateOnly WorkDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? ShiftName,
    DateTimeOffset CreatedAt);

/// <summary>
/// 排班分页查询入参
/// </summary>
public class ScheduleQueryInput : PageRequest
{
    /// <summary>
    /// 用户ID筛选
    /// </summary>
    public UserId? UserId { get; set; }
    /// <summary>
    /// 工作日期起
    /// </summary>
    public DateOnly? WorkDateFrom { get; set; }
    /// <summary>
    /// 工作日期止
    /// </summary>
    public DateOnly? WorkDateTo { get; set; }
}

/// <summary>
/// 排班查询服务
/// </summary>
public class ScheduleQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询排班
    /// </summary>
    public async Task<ScheduleQueryDto?> GetByIdAsync(ScheduleId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Schedules
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ScheduleQueryDto(s.Id, s.UserId, s.WorkDate, s.StartTime, s.EndTime, s.ShiftName, s.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询排班
    /// </summary>
    public async Task<PagedData<ScheduleQueryDto>> GetPagedAsync(ScheduleQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Schedules.AsNoTracking();
        if (input.UserId != null)
            query = query.Where(s => s.UserId == input.UserId);
        if (input.WorkDateFrom.HasValue)
            query = query.Where(s => s.WorkDate >= input.WorkDateFrom.Value);
        if (input.WorkDateTo.HasValue)
            query = query.Where(s => s.WorkDate <= input.WorkDateTo.Value);

        return await query
            .OrderBy(s => s.WorkDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new ScheduleQueryDto(s.Id, s.UserId, s.WorkDate, s.StartTime, s.EndTime, s.ShiftName, s.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
