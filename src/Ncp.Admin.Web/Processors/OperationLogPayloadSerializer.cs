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

    private static object Normalize(object value)
    {
        if (value is string s) return s;
        if (value is byte[] bytes) return $"[bytes:{bytes.Length}]";
        if (value is Stream) return "[stream]";
        return value;
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
        => s.Length <= maxLen ? s : s[..maxLen];
}

