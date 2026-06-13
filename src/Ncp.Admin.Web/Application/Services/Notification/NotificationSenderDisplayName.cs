namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 站内通知发布人展示名解析（与定时任务、领域事件写入的 <see cref="SystemStoredName"/> 约定一致）。
/// </summary>
public static class NotificationSenderDisplayName
{
    /// <summary>库内系统/自动通知写入的发送人名称。</summary>
    public const string SystemStoredName = "系统";

    /// <summary>面向用户展示的默认发布人名称（微信模板等无 i18n 场景使用）。</summary>
    public const string DisplayName = "OA系统";

    /// <summary>
    /// 是否为系统发起或无发布人。
    /// </summary>
    public static bool IsSystemSender(string? senderName)
    {
        if (string.IsNullOrWhiteSpace(senderName))
        {
            return true;
        }

        var trimmed = senderName.Trim();
        return string.Equals(trimmed, SystemStoredName, StringComparison.Ordinal)
               || string.Equals(trimmed, DisplayName, StringComparison.Ordinal);
    }

    /// <summary>
    /// 解析通知发布人展示名；系统或无发布人时返回 <see cref="DisplayName"/>。
    /// </summary>
    public static string Resolve(string? senderName)
    {
        if (IsSystemSender(senderName))
        {
            return DisplayName;
        }

        return senderName!.Trim();
    }
}
