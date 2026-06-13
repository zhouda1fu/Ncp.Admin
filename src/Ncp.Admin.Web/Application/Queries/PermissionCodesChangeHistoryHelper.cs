using System.Globalization;
using System.Text.Json;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>权限码列表在修改记录中的展示与对比。</summary>
internal static class PermissionCodesChangeHistoryHelper
{
    public static (string OldDisplay, string NewDisplay) FormatPermissionDiff(
        JsonDocument? previousDoc,
        string? previousRequestBody,
        JsonDocument? currentDoc,
        string? currentRequestBody)
    {
        var oldCodes = ExtractPermissionCodes(previousDoc, previousRequestBody);
        var newCodes = ExtractPermissionCodes(currentDoc, currentRequestBody);
        if (oldCodes.SetEquals(newCodes))
            return ("—", "—");

        return (FormatPermissionCodesDisplay(oldCodes), FormatPermissionCodesDisplay(newCodes));
    }

    public static string FormatPermissionCodesDisplay(IEnumerable<string> codes)
    {
        var names = codes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .Select(c =>
            {
                var (name, _) = PermissionMapper.GetPermissionInfo(c);
                return string.IsNullOrWhiteSpace(name) ? c : name;
            })
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();

        return names.Length == 0 ? "—" : string.Join("、", names);
    }

    public static HashSet<string> ExtractPermissionCodes(JsonDocument? doc, string? rawBody)
    {
        if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            var root = GetPropertyBagRoot(doc.RootElement);
            if (ChangeHistoryJson.TryGetProperty(root, "permissionCodes", out var el))
                return ExtractPermissionCodesFromElement(el);
        }

        return ExtractPermissionCodesFromRaw(rawBody);
    }

    private static HashSet<string> ExtractPermissionCodesFromElement(JsonElement el)
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        codes.Add(s.Trim());
                }
            }

            return codes;
        }

        if (el.ValueKind != JsonValueKind.Object)
            return codes;

        if (ChangeHistoryJson.TryGetProperty(el, "codes", out var codesEl) && codesEl.ValueKind == JsonValueKind.String)
            AddDelimitedCodes(codesEl.GetString(), codes);
        else if (ChangeHistoryJson.TryGetProperty(el, "items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in itemsEl.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        codes.Add(s.Trim());
                }
            }
        }

        return codes;
    }

    private static HashSet<string> ExtractPermissionCodesFromRaw(string? rawBody)
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(rawBody))
            return codes;

        var objectMatch = System.Text.RegularExpressions.Regex.Match(
            rawBody,
            "\"permissionCodes\"\\s*:\\s*\\{(.*?)\\}",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
            | System.Text.RegularExpressions.RegexOptions.Singleline
            | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (objectMatch.Success)
        {
            var objectBody = objectMatch.Groups[1].Value;
            var codesMatch = System.Text.RegularExpressions.Regex.Match(
                objectBody,
                "\"codes\"\\s*:\\s*\"([^\"]+)\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            if (codesMatch.Success)
                AddDelimitedCodes(codesMatch.Groups[1].Value, codes);

            if (codes.Count > 0)
                return codes;
        }

        var arrayMatch = System.Text.RegularExpressions.Regex.Match(
            rawBody,
            "\"permissionCodes\"\\s*:\\s*\\[(.*?)\\]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
            | System.Text.RegularExpressions.RegexOptions.Singleline
            | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (!arrayMatch.Success)
            return codes;

        foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                     arrayMatch.Groups[1].Value,
                     "\"((?:\\\\.|[^\"\\\\])*)\"",
                     System.Text.RegularExpressions.RegexOptions.CultureInvariant))
        {
            var value = System.Text.RegularExpressions.Regex.Unescape(m.Groups[1].Value).Trim();
            if (!string.IsNullOrWhiteSpace(value))
                codes.Add(value);
        }

        return codes;
    }

    private static void AddDelimitedCodes(string? text, HashSet<string> codes)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(part))
                codes.Add(part);
        }
    }

    private static JsonElement GetPropertyBagRoot(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return root;
        if (ChangeHistoryJson.TryGetProperty(root, "data", out var data) && data.ValueKind == JsonValueKind.Object)
            return data;
        return root;
    }

    internal static string FormatPermissionCountFromRaw(string? rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return string.Empty;

        var countMatch = System.Text.RegularExpressions.Regex.Match(
            rawBody,
            "\"permissionCodes\"\\s*:\\s*\\{[^}]*\"count\"\\s*:\\s*(\\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (countMatch.Success)
            return countMatch.Groups[1].Value;

        return CountJsonArrayItems(rawBody, "permissionCodes").ToString(CultureInfo.InvariantCulture);
    }

    private static int CountJsonArrayItems(string rawBody, string key)
    {
        var start = rawBody.IndexOf($"\"{key}\"", StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return 0;
        var bracketStart = rawBody.IndexOf('[', start);
        if (bracketStart < 0)
            return 0;

        var depth = 0;
        var inString = false;
        var escaped = false;
        var itemCount = 0;
        var hasItem = false;
        for (var i = bracketStart; i < rawBody.Length; i++)
        {
            var ch = rawBody[i];
            if (inString)
            {
                if (escaped)
                    escaped = false;
                else if (ch == '\\')
                    escaped = true;
                else if (ch == '"')
                    inString = false;
                continue;
            }

            if (ch == '"')
            {
                inString = true;
                hasItem = true;
                continue;
            }

            if (ch == '[')
            {
                depth++;
                continue;
            }

            if (ch == ']')
            {
                depth--;
                if (depth == 0)
                {
                    if (hasItem)
                        itemCount++;
                    break;
                }
                continue;
            }

            if (depth == 1 && ch == ',')
            {
                if (hasItem)
                    itemCount++;
                hasItem = false;
            }
        }

        return itemCount;
    }
}
