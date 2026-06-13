using System.Globalization;
using System.Text.Json;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>操作日志请求体 JSON 解析辅助。</summary>
internal static class ChangeHistoryJson
{
    public static JsonElement GetPropertyBagRoot(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return root;
        if (TryGetProperty(root, "data", out var data) && data.ValueKind == JsonValueKind.Object)
            return data;
        return root;
    }

    public static bool TryGetProperty(JsonElement obj, string camelName, out JsonElement value)
    {
        if (obj.TryGetProperty(camelName, out value))
            return true;
        if (camelName.Length > 0)
        {
            var pascal = char.ToUpperInvariant(camelName[0]) + camelName[1..];
            if (obj.TryGetProperty(pascal, out value))
                return true;
        }

        value = default;
        return false;
    }

    public static JsonDocument? TryParse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        try
        {
            return JsonDocument.Parse(body);
        }
        catch
        {
            return null;
        }
    }

    public static bool TryReadGuidElement(JsonElement idEl, out string id)
    {
        id = string.Empty;
        if (idEl.ValueKind == JsonValueKind.String)
        {
            id = idEl.GetString() ?? string.Empty;
            return Guid.TryParse(id, out _);
        }

        if (idEl.ValueKind == JsonValueKind.Object)
        {
            if (TryGetProperty(idEl, "value", out var vEl))
                return TryReadGuidElement(vEl, out id);
            if (TryGetProperty(idEl, "id", out var innerIdEl))
                return TryReadGuidElement(innerIdEl, out id);
        }

        if (idEl.TryGetGuid(out var g))
        {
            id = g.ToString("D");
            return true;
        }

        return false;
    }

    public static bool TryReadInt64Element(JsonElement idEl, out long id)
    {
        if (idEl.TryGetInt64(out id))
            return true;
        if (idEl.ValueKind == JsonValueKind.String
            && long.TryParse(idEl.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
            return true;
        if (idEl.ValueKind == JsonValueKind.Object)
        {
            if (TryGetProperty(idEl, "id", out var innerIdEl))
                return TryReadInt64Element(innerIdEl, out id);
            if (TryGetProperty(idEl, "value", out var valueEl))
                return TryReadInt64Element(valueEl, out id);
        }

        id = 0;
        return false;
    }

    public static string NormalizeForCompare(JsonElement el)
    {
        if (el.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return string.Empty;
        if (el.ValueKind == JsonValueKind.Array)
        {
            var parts = el.EnumerateArray()
                .Select(e => e.ValueKind == JsonValueKind.String ? (e.GetString() ?? string.Empty) : e.GetRawText())
                .Where(s => !string.IsNullOrEmpty(s))
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToArray();
            return string.Join(",", parts);
        }

        if (el.ValueKind == JsonValueKind.String)
            return el.GetString() ?? string.Empty;
        return el.GetRawText();
    }

    public static bool TryCoerceInt32(JsonElement el, out int value)
    {
        if (el.TryGetInt32(out value))
            return true;
        if (el.ValueKind == JsonValueKind.String
            && int.TryParse(el.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;
        if (el.TryGetInt64(out var lv))
        {
            value = (int)lv;
            return true;
        }

        value = 0;
        return false;
    }

    public static bool TryCoerceBoolean(JsonElement el, out bool value)
    {
        if (el.ValueKind == JsonValueKind.True)
        {
            value = true;
            return true;
        }

        if (el.ValueKind == JsonValueKind.False)
        {
            value = false;
            return true;
        }

        if (el.ValueKind == JsonValueKind.String
            && bool.TryParse(el.GetString(), out value))
            return true;

        value = false;
        return false;
    }
}
