using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;

/// <summary>
/// 考勤记录ID（强类型ID）
/// </summary>
public partial record AttendanceRecordId : IGuidStronglyTypedId;

/// <summary>
/// 打卡来源
/// </summary>
public enum AttendanceSource
{
    /// <summary>
    /// GPS 定位打卡
    /// </summary>
    Gps = 0,
    /// <summary>
    /// WiFi 打卡
    /// </summary>
    Wifi = 1,
    /// <summary>
    /// 手动签到
    /// </summary>
    Manual = 2,
}

/// <summary>
/// 考勤记录聚合根，表示一次打卡/签退记录
/// </summary>
public class AttendanceRecord : Entity<AttendanceRecordId>, IAggregateRoot
{
    protected AttendanceRecord() { }

    /// <summary>
    /// 打卡用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;
    /// <summary>
    /// 签到时间
    /// </summary>
    public DateTimeOffset CheckInAt { get; private set; }
    /// <summary>
    /// 签退时间，未签退时为 null
    /// </summary>
    public DateTimeOffset? CheckOutAt { get; private set; }
    /// <summary>
    /// 打卡来源（GPS/WiFi/手动）
    /// </summary>
    public AttendanceSource Source { get; private set; }
    /// <summary>
    /// 打卡地点描述（可选）
    /// </summary>
    public string? Location { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 创建考勤记录（签到）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="checkInAt">签到时间</param>
    /// <param name="source">打卡来源</param>
    /// <param name="location">地点（可选）</param>
    public AttendanceRecord(UserId userId, DateTimeOffset checkInAt, AttendanceSource source, string? location = null)
    {
        UserId = userId;
        CheckInAt = checkInAt;
        Source = source;
        Location = location;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 签退，记录签退时间
    /// </summary>
    /// <param name="checkOutAt">签退时间</param>
    public void CheckOut(DateTimeOffset checkOutAt)
    {
        CheckOutAt = checkOutAt;
    }
}
