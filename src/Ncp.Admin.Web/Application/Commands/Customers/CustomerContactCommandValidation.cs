using System.Text.RegularExpressions;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 客户联系人命令共用的校验规则（新增/更新）
/// </summary>
internal static class CustomerContactCommandValidation
{
    internal const string AtLeastOneChannelMessage = "手机号、座机、QQ、微信至少填写一项";
    internal const string MobileFormatMessage = "手机号格式不正确";
    internal const string QqFormatMessage = "QQ号格式不正确";

    /// <summary>中国大陆手机号：1 开头 11 位</summary>
    private static readonly Regex MobileRegex = new(@"^1[3-9]\d{9}$", RegexOptions.Compiled);

    /// <summary>QQ：5–11 位数字，首位非 0</summary>
    private static readonly Regex QqRegex = new(@"^[1-9]\d{4,10}$", RegexOptions.Compiled);

    internal static bool HasAtLeastOneContactChannel(string? mobile, string? phone, string? qq, string? wechat) =>
        !string.IsNullOrWhiteSpace(mobile)
        || !string.IsNullOrWhiteSpace(phone)
        || !string.IsNullOrWhiteSpace(qq)
        || !string.IsNullOrWhiteSpace(wechat);

    internal static bool IsValidMobileIfPresent(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return true;
        return MobileRegex.IsMatch(mobile.Trim());
    }

    internal static bool IsValidQqIfPresent(string? qq)
    {
        if (string.IsNullOrWhiteSpace(qq)) return true;
        return QqRegex.IsMatch(qq.Trim());
    }
}
