using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

/// <summary>
/// 用车预订ID（强类型ID）
/// </summary>
public partial record VehicleBookingId : IGuidStronglyTypedId;

/// <summary>
/// 预订状态
/// </summary>
public enum VehicleBookingStatus
{
    /// <summary>
    /// 已预订
    /// </summary>
    Booked = 0,
    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 1,
    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2,
}

/// <summary>
/// 用车预订聚合根：预约、调度
/// </summary>
public class VehicleBooking : Entity<VehicleBookingId>, IAggregateRoot
{
    protected VehicleBooking() { }

    /// <summary>
    /// 车辆ID
    /// </summary>
    public VehicleId VehicleId { get; private set; } = default!;
    /// <summary>
    /// 预订人用户ID
    /// </summary>
    public UserId BookerId { get; private set; } = default!;
    /// <summary>
    /// 用车事由/目的地
    /// </summary>
    public string Purpose { get; private set; } = string.Empty;
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset StartAt { get; private set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset EndAt { get; private set; }
    /// <summary>
    /// 状态
    /// </summary>
    public VehicleBookingStatus Status { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建用车预订
    /// </summary>
    public VehicleBooking(VehicleId vehicleId, UserId bookerId, string purpose, DateTimeOffset startAt, DateTimeOffset endAt)
    {
        VehicleId = vehicleId;
        BookerId = bookerId;
        Purpose = purpose ?? string.Empty;
        StartAt = startAt;
        EndAt = endAt;
        Status = VehicleBookingStatus.Booked;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 取消预订
    /// </summary>
    public void Cancel()
    {
        if (Status != VehicleBookingStatus.Booked)
            throw new KnownException("仅已预订可取消", ErrorCodes.VehicleBookingInvalidStatus);
        Status = VehicleBookingStatus.Cancelled;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 完成
    /// </summary>
    public void Complete()
    {
        if (Status != VehicleBookingStatus.Booked)
            throw new KnownException("仅已预订可完成", ErrorCodes.VehicleBookingInvalidStatus);
        Status = VehicleBookingStatus.Completed;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
