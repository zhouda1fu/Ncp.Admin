using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;

/// <summary>
/// 排班ID（强类型ID）
/// </summary>
public partial record ScheduleId : IGuidStronglyTypedId;

/// <summary>
/// 排班聚合根，表示用户某日的班次（上班/下班时间）
/// </summary>
public class Schedule : Entity<ScheduleId>, IAggregateRoot
{
    protected Schedule() { }

    /// <summary>
    /// 排班用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;
    /// <summary>
    /// 工作日期
    /// </summary>
    public DateOnly WorkDate { get; private set; }
    /// <summary>
    /// 班次开始时间
    /// </summary>
    public TimeOnly StartTime { get; private set; }
    /// <summary>
    /// 班次结束时间
    /// </summary>
    public TimeOnly EndTime { get; private set; }
    /// <summary>
    /// 班次名称（如早班、晚班，可选）
    /// </summary>
    public string? ShiftName { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建排班
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="workDate">工作日期</param>
    /// <param name="startTime">上班时间</param>
    /// <param name="endTime">下班时间</param>
    /// <param name="shiftName">班次名称（可选）</param>
    public Schedule(UserId userId, DateOnly workDate, TimeOnly startTime, TimeOnly endTime, string? shiftName = null)
    {
        UserId = userId;
        WorkDate = workDate;
        StartTime = startTime;
        EndTime = endTime;
        ShiftName = shiftName;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新班次时间与名称
    /// </summary>
    /// <param name="startTime">上班时间</param>
    /// <param name="endTime">下班时间</param>
    /// <param name="shiftName">班次名称（可选）</param>
    public void UpdateShift(TimeOnly startTime, TimeOnly endTime, string? shiftName = null)
    {
        StartTime = startTime;
        EndTime = endTime;
        ShiftName = shiftName;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
