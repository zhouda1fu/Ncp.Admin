using System.Globalization;
using System.Text.Json;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 根据流程变量 JSON 与设计器条件列表进行分支判断。
/// 支持运算符：&gt;, &lt;, &gt;=, &lt;=, ==, !=, include, notinclude。
/// 支持类型：数值、字符串、布尔值。
/// </summary>
public static class WorkflowConditionEvaluator
{
    /// <summary>
    /// 评估设计器条件列表：组间 OR，组内 AND。空列表或空组视为不命中。
    /// </summary>
    public static bool EvaluateDesignerConditionList(string? variablesJson, List<List<DesignerConditionRule>>? conditionList)
    {
        if (conditionList == null || conditionList.Count == 0) return false;

        JsonElement? root = null;
        if (!string.IsNullOrWhiteSpace(variablesJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(variablesJson);
                root = doc.RootElement.Clone();
            }
            catch
            {
                // 无效 JSON 时当作空变量
            }
        }

        foreach (var group in conditionList)
        {
            if (group == null || group.Count == 0) continue;
            var allTrue = true;
            foreach (var rule in group)
            {
                if (rule == null || !EvaluateDesignerRule(root, rule))
                {
                    allTrue = false;
                    break;
                }
            }
            if (allTrue) return true;
        }
        return false;
    }

    private static bool EvaluateDesignerRule(JsonElement? variables, DesignerConditionRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Field)) return false;
        if (!variables.HasValue) return false;
        if (!TryGetPropertyByPath(variables.Value, rule.Field, out var prop))
            return false;
        var valueStr = (rule.Value ?? string.Empty).Trim().Trim('"');

        if (prop.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            var leftBool = prop.ValueKind == JsonValueKind.True;
            if (!bool.TryParse(valueStr, out var rightBool)) return false;
            return rule.Operator switch
            {
                "==" => leftBool == rightBool,
                "!=" => leftBool != rightBool,
                _ => false
            };
        }
        if (prop.ValueKind == JsonValueKind.Number)
        {
            if (!double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var numRight))
                return false;
            return CompareNumeric(prop.GetDouble(), numRight, rule.Operator);
        }
        var leftStr = prop.ValueKind == JsonValueKind.String ? prop.GetString() ?? "" : prop.GetRawText();
        return CompareString(leftStr, valueStr, rule.Operator);
    }

    private static bool TryGetPropertyByPath(JsonElement root, string field, out JsonElement value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(field)) return false;

        // Support nested properties like: CategoryDiscountPoints.<ProductCategoryIdGuid>
        // Only JSON objects are supported; arrays are not.
        var current = root;
        var parts = field.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return false;

        foreach (var part in parts)
        {
            if (current.ValueKind != JsonValueKind.Object) return false;
            if (!current.TryGetProperty(part, out var next)) return false;
            current = next;
        }

        value = current;
        return true;
    }

    private static bool CompareNumeric(double left, double right, string op)
    {
        return op switch
        {
            ">" => left > right,
            "<" => left < right,
            ">=" => left >= right,
            "<=" => left <= right,
            "==" => Math.Abs(left - right) < 1e-9,
            "!=" => Math.Abs(left - right) >= 1e-9,
            _ => false
        };
    }

    private static bool CompareString(string left, string right, string op)
    {
        var cmp = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        return op switch
        {
            "==" => cmp == 0,
            "!=" => cmp != 0,
            ">" => cmp > 0,
            "<" => cmp < 0,
            ">=" => cmp >= 0,
            "<=" => cmp <= 0,
            "include" => left.Contains(right, StringComparison.OrdinalIgnoreCase),
            "notinclude" => !left.Contains(right, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}
