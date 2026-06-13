using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;

/// <summary>
/// 用户首页卡片排序偏好（每用户一条）。
/// </summary>
public class UserHomeDashboardPreference : Entity<UserId>, IAggregateRoot
{
    protected UserHomeDashboardPreference()
    {
    }

    public UserHomeDashboardPreference(UserId userId, string cardOrderJson)
    {
        Id = userId;
        CardOrderJson = cardOrderJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 可见卡片 key 的有序 JSON 数组，如 ["announcement","process",...]。
    /// </summary>
    public string CardOrderJson { get; private set; } = "[]";

    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateCardOrder(string cardOrderJson)
    {
        CardOrderJson = cardOrderJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
