namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户联络记录状态
/// </summary>
public enum CustomerContactRecordStatus
{
    /// <summary>待选择</summary>
    Pending = 0,
    /// <summary>有效联系</summary>
    Effective = 1,
    /// <summary>无效联系</summary>
    Ineffective = 2,
}

/// <summary>
/// 客户联络记录类型（存库为 int）
/// </summary>
public enum CustomerContactRecordType
{
    /// <summary>电话</summary>
    Phone = 1,
    /// <summary>出差（原上门拜访）</summary>
    BusinessTrip = 2,
    /// <summary>微信</summary>
    Wechat = 3,
    /// <summary>其他</summary>
    Other = 4,
}

/// <summary>
/// 提醒间隔（天，与 <see cref="CustomerContactRecordReminderInterval"/> 允许取值一致）
/// </summary>
public enum CustomerContactRecordReminderIntervalDays
{
    /// <summary>1 天</summary>
    OneDay = 1,
    /// <summary>2 天</summary>
    TwoDays = 2,
    /// <summary>3 天</summary>
    ThreeDays = 3,
    /// <summary>10 天</summary>
    TenDays = 10,
    /// <summary>15 天</summary>
    FifteenDays = 15,
    /// <summary>20 天</summary>
    TwentyDays = 20,
    /// <summary>30 天</summary>
    ThirtyDays = 30,
    /// <summary>50 天</summary>
    FiftyDays = 50,
    /// <summary>80 天</summary>
    EightyDays = 80,
    /// <summary>100 天</summary>
    OneHundredDays = 100,
}

/// <summary>
/// 客户联络记录提醒间隔取值约束（与库内 int 列对应）
/// </summary>
public static class CustomerContactRecordReminderInterval
{
    public static readonly int[] AllowedValues = [1, 2, 3, 10, 15, 20, 30, 50, 80, 100];

    public static bool IsValid(int days) => AllowedValues.Contains(days);
}
