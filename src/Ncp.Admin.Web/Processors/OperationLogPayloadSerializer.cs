using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace Ncp.Admin.Web.Processors;

internal static class OperationLogPayloadSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "pwd",
        "token",
        "refreshToken",
        "accessToken",
        "authorization",
        "idCardNumber",
    };

    public static string SerializeMasked(object? value, int maxLen)
    {
        if (value == null) return string.Empty;
        try
        {
            var normalized = Normalize(value);
            var json = JsonSerializer.Serialize(normalized, Options);
            var masked = MaskJson(json);
            return Trunc(masked, maxLen);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static object? Normalize(object value) => Sanitize(value);

    /// <summary>
    /// 递归清理对象图，将 IFormFile、Stream、byte[] 等不可序列化或敏感的大对象替换为占位符，避免序列化异常导致上传等接口返回 500。
    /// </summary>
    private static object? Sanitize(object? value)
    {
        if (value == null) return null;
        if (value is string s) return s;
        if (value is byte[] bytes) return $"[bytes:{bytes.Length}]";
        if (value is Stream) return "[stream]";

        var type = value.GetType();
        // 任何 IFormFile 或类似文件类型直接替换
        if (type.Name.Contains("FormFile", StringComparison.OrdinalIgnoreCase) ||
            type.FullName?.Contains("IFormFile", StringComparison.OrdinalIgnoreCase) == true)
            return "[file-upload]";

        // 简单值类型直接返回
        if (type.IsPrimitive || value is decimal || value is DateTime || value is DateTimeOffset || value is Guid)
            return value;

        // 强类型 ID（如 RoleId/DeptId）：仅存 Guid 字符串，便于审计解析且减少体积
        var idProp = type.GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (idProp?.PropertyType == typeof(Guid) && idProp.GetValue(value) is Guid guidId)
            return guidId.ToString("D");

        // 数组/集合：递归处理元素
        if (value is System.Collections.IEnumerable enumerable && !(value is string))
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
                list.Add(Sanitize(item));

            // 大列表：优先用逗号分隔完整值，便于修改记录展示
            if (list.Count > 30 && list.All(static x => x is string))
            {
                var codes = list.Cast<string>().Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                var compact = string.Join(',', codes);
                var allGuids = codes.All(static x => Guid.TryParse(x, out _));
                var valueKey = allGuids ? "ids" : "codes";
                if (compact.Length <= 2800)
                {
                    return new Dictionary<string, object?>
                    {
                        ["count"] = codes.Count,
                        [valueKey] = compact,
                    };
                }

                return new Dictionary<string, object?>
                {
                    ["count"] = codes.Count,
                    ["items"] = codes.Take(40).ToList(),
                    ["truncated"] = true,
                };
            }

            return list;
        }

        // 普通对象：尝试按属性递归清理（仅公共实例属性）
        try
        {
            var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (props.Length == 0) return value; // 无属性则原样（可能是未知值类型）

            var dict = new Dictionary<string, object?>();
            foreach (var p in props)
            {
                if (!p.CanRead) continue;
                var val = p.GetValue(value);
                dict[p.Name] = Sanitize(val);
            }
            return dict;
        }
        catch
        {
            return "[object]";
        }
    }

    private static string MaskJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false,
            });
            WriteMasked(doc.RootElement, writer);
            writer.Flush();
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        catch
        {
            return json;
        }
    }

    private static void WriteMasked(JsonElement el, Utf8JsonWriter w)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                w.WriteStartObject();
                foreach (var p in el.EnumerateObject())
                {
                    w.WritePropertyName(p.Name);
                    if (SensitiveKeys.Contains(p.Name))
                        w.WriteStringValue("***");
                    else
                        WriteMasked(p.Value, w);
                }
                w.WriteEndObject();
                break;
            case JsonValueKind.Array:
                w.WriteStartArray();
                foreach (var item in el.EnumerateArray())
                    WriteMasked(item, w);
                w.WriteEndArray();
                break;
            default:
                el.WriteTo(w);
                break;
        }
    }

    private static string Trunc(string s, int maxLen)
    {
        if (s.Length <= maxLen)
            return s;

        // 截断时尽量闭合 JSON，避免修改记录等场景无法解析
        var cut = s[..maxLen];
        var openBraces = cut.Count(c => c == '{') - cut.Count(c => c == '}');
        var openBrackets = cut.Count(c => c == '[') - cut.Count(c => c == ']');
        if (openBrackets > 0)
            cut += new string(']', openBrackets);
        if (openBraces > 0)
            cut += new string('}', openBraces);
        return cut.Length <= maxLen ? cut : cut[..maxLen];
    }
}

