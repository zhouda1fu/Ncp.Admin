using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;

/// <summary>
/// 请假余额ID（强类型ID）
/// </summary>
public partial record LeaveBalanceId : IGuidStronglyTypedId;

/// <summary>
/// 请假余额聚合根
/// 按用户、年度、请假类型维护额度与已用天数
/// </summary>
public class LeaveBalance : Entity<LeaveBalanceId>, IAggregateRoot
{
    protected LeaveBalance()
    {
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// 年度（如 2026）
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// 请假类型
    /// </summary>
    public LeaveRequestAggregate.LeaveType LeaveType { get; private set; }

    /// <summary>
    /// 总天数（如年假 10 天）
    /// </summary>
    public decimal TotalDays { get; private set; }

    /// <summary>
    /// 已使用天数
    /// </summary>
    public decimal UsedDays { get; private set; }

    /// <summary>
    /// 剩余天数
    /// </summary>
    public decimal RemainingDays => TotalDays - UsedDays;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建或获取余额记录（由应用层保证唯一 UserId+Year+LeaveType）
    /// </summary>
    public LeaveBalance(UserId userId, int year, LeaveRequestAggregate.LeaveType leaveType, decimal totalDays)
    {
        UserId = userId;
        Year = year;
        LeaveType = leaveType;
        TotalDays = totalDays;
        UsedDays = 0;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 扣减额度（审批通过时调用）
    /// </summary>
    public void Deduct(decimal days)
    {
        if (days <= 0)
        {
            throw new KnownException("扣减天数必须大于0", ErrorCodes.LeaveBalanceInvalidDeduct);
        }

        if (UsedDays + days > TotalDays)
        {
            throw new KnownException("请假余额不足", ErrorCodes.LeaveBalanceInsufficient);
        }

        UsedDays += days;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 调整总天数（管理员维护年度额度）
    /// </summary>
    public void SetTotalDays(decimal totalDays)
    {
        if (totalDays < UsedDays)
        {
            throw new KnownException("总天数不能小于已使用天数", ErrorCodes.LeaveBalanceInvalidTotal);
        }

        TotalDays = totalDays;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
