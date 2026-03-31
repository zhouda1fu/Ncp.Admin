using System.Globalization;

namespace Ncp.Admin.Web.Application.Identity.Admin.UserExcel;

internal static class UserImportParsing
{
    public static bool TryParseBool(string? s, out bool value)
    {
        value = false;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var t = s.Trim();
        if (t is "是" or "1" or "true" or "TRUE" or "True" or "yes" or "YES" or "Y" or "y")
        {
            value = true;
            return true;
        }

        if (t is "否" or "0" or "false" or "FALSE" or "False" or "no" or "NO" or "N" or "n")
        {
            value = false;
            return true;
        }

        return bool.TryParse(t, out value);
    }

    public static bool TryParseInt(string? s, out int value)
    {
        value = 0;
        return !string.IsNullOrWhiteSpace(s) && int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDateTimeOffset(string? s, out DateTimeOffset value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var t = s.Trim();
        if (DateTimeOffset.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out value))
        {
            return true;
        }

        return DateTimeOffset.TryParse(t, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out value);
    }

    public static IEnumerable<string> SplitRoleNames(string? rolesCell)
    {
        if (string.IsNullOrWhiteSpace(rolesCell))
        {
            return [];
        }

        return rolesCell
            .Split([',', '，', ';', '；'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0);
    }
}
