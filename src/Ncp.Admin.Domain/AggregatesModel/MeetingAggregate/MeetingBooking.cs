using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;

/// <summary>
/// 会议室预订ID（强类型ID）
/// </summary>
public partial record MeetingBookingId : IGuidStronglyTypedId;

/// <summary>
/// 预订状态
/// </summary>
public enum MeetingBookingStatus
{
    /// <summary>
    /// 已预订
    /// </summary>
    Booked = 0,
    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 1,
}

/// <summary>
/// 会议室预订聚合根，表示某会议室在某一时段的预订
/// </summary>
public class MeetingBooking : Entity<MeetingBookingId>, IAggregateRoot
{
    protected MeetingBooking() { }

    /// <summary>
    /// 会议室ID
    /// </summary>
    public MeetingRoomId MeetingRoomId { get; private set; } = default!;
    /// <summary>
    /// 预订人用户ID
    /// </summary>
    public UserId BookerId { get; private set; } = default!;
    /// <summary>
    /// 会议主题/标题
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset StartAt { get; private set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset EndAt { get; private set; }
    /// <summary>
    /// 预订状态
    /// </summary>
    public MeetingBookingStatus Status { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建会议室预订
    /// </summary>
    /// <param name="meetingRoomId">会议室ID</param>
    /// <param name="bookerId">预订人用户ID</param>
    /// <param name="title">会议主题</param>
    /// <param name="startAt">开始时间</param>
    /// <param name="endAt">结束时间</param>
    public MeetingBooking(MeetingRoomId meetingRoomId, UserId bookerId, string title, DateTimeOffset startAt, DateTimeOffset endAt)
    {
        MeetingRoomId = meetingRoomId;
        BookerId = bookerId;
        Title = title ;
        StartAt = startAt;
        EndAt = endAt;
        Status = MeetingBookingStatus.Booked;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 取消预订
    /// </summary>
    public void Cancel()
    {
        Status = MeetingBookingStatus.Cancelled;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
