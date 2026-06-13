using System.Globalization;

namespace Ncp.Admin.Web.Extensions;

public static class StringExtensions
{
    private static readonly CultureInfo ChineseCulture = CultureInfo.GetCultureInfo("zh-CN");

    public static string TrimToEmpty(this string? value) => value?.Trim() ?? string.Empty;

    public static DateTimeOffset? ParseNullableDateTimeOffset(this string? value)
    {
        return value.TryParseDateTimeOffset(out var dateTimeOffset) ? dateTimeOffset : null;
    }

    public static bool TryParseDateTimeOffset(this string? value, out DateTimeOffset dateTimeOffset)
    {
        dateTimeOffset = default;
        var text = value.TrimToEmpty();
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (DateTimeOffset.TryParse(text, ChineseCulture, DateTimeStyles.AssumeLocal, out dateTimeOffset))
            return true;

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTimeOffset))
            return true;

        if (DateTime.TryParse(text, ChineseCulture, DateTimeStyles.None, out var dateTime))
        {
            dateTimeOffset = new DateTimeOffset(dateTime);
            return true;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            dateTimeOffset = new DateTimeOffset(dateTime);
            return true;
        }

        var dateFormats = new[]
        {
            "yyyy-MM-dd",
            "yyyy/M/d",
            "yyyy/MM/dd",
            "yyyy/M/dd",
            "yyyy/MM/d",
        };
        if (DateTime.TryParseExact(
                text,
                dateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateTime))
        {
            dateTimeOffset = new DateTimeOffset(dateTime);
            return true;
        }

        return false;
    }
}
