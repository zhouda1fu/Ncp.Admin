using System.Globalization;
using System.Text.Json;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 根据流程变量 JSON 与设计器条件列表进行分支判断。
/// 支持运算符：&gt;, &lt;, &gt;=, &lt;=, ==, !=, include, notinclude。
/// 字符串比较时，若 <c>==</c> / <c>!=</c> 右侧含英文逗号，则按「逗号分隔多值」解析：<c>==</c> 表示左侧等于其中任一，<c>!=</c> 表示左侧不等于其中任一（用于路由角色多选）。
/// 支持类型：数值、字符串、布尔值。
/// 订单变量 <c>CategoryDiscountPoints.&lt;产品分类Id&gt;</c>：若订单未包含该分类的优惠行，则 JSON 中无对应键，条件求值时按数值 <c>0</c> 处理（便于书写「优惠-微 == 0」）。
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
        if (TryEvaluateOrderIdPresenceRule(variables, rule, out var presenceResult))
        {
            return presenceResult;
        }

        if (!variables.HasValue) return false;
        if (!TryGetPropertyByPath(variables.Value, rule.Field, out var prop))
        {
            if (TryEvaluateMissingCategoryDiscountPointsAsZero(variables.Value, rule, out var ruleResult))
            {
                return ruleResult;
            }

            return false;
        }

        var valueStr = (rule.Value ?? string.Empty).Trim().Trim('"');
        var op = (rule.Operator ?? string.Empty).Trim();

        if (prop.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            var leftBool = prop.ValueKind == JsonValueKind.True;
            if (!bool.TryParse(valueStr, out var rightBool)) return false;
            return op switch
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
            return CompareNumeric(prop.GetDouble(), numRight, op);
        }
        var leftStr = prop.ValueKind == JsonValueKind.String ? prop.GetString() ?? "" : prop.GetRawText();
        return CompareString(leftStr, valueStr, op);
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
            if (!TryGetPropertyInsensitive(current, part, out var next)) return false;
            current = next;
        }

        value = current;
        return true;
    }

    /// <summary>
    /// 订单工作流变量 <c>CategoryDiscountPoints</c> 中未出现的分类：视为优惠点数 0。
    /// </summary>
    /// <summary>
    /// 办公任务变量 <c>OrderId</c>：值为 empty / notempty，运算符须为 ==，判断关联订单 ID 是否为空。
    /// </summary>
    private static bool TryEvaluateOrderIdPresenceRule(
        JsonElement? variables,
        DesignerConditionRule rule,
        out bool ruleResult)
    {
        ruleResult = false;
        var field = rule.Field ?? string.Empty;
        if (!string.Equals(field, "OrderId", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(rule.Operator, "==", StringComparison.Ordinal))
        {
            return false;
        }

        var mode = (rule.Value ?? string.Empty).Trim().Trim('"');
        if (!mode.Equals("empty", StringComparison.OrdinalIgnoreCase)
            && !mode.Equals("notempty", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var linked = IsOfficeTaskOrderIdPresent(variables);
        ruleResult = mode.Equals("notempty", StringComparison.OrdinalIgnoreCase) ? linked : !linked;
        return true;
    }

    private static bool IsOfficeTaskOrderIdPresent(JsonElement? variables)
    {
        if (!variables.HasValue)
        {
            return false;
        }

        if (TryGetPropertyByPath(variables.Value, "OrderId", out var orderIdProp)
            && TryReadNonEmptyString(orderIdProp, out var orderId)
            && !string.Equals(orderId, Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool TryReadNonEmptyString(JsonElement prop, out string value)
    {
        value = string.Empty;
        if (prop.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = prop.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryEvaluateMissingCategoryDiscountPointsAsZero(
        JsonElement root,
        DesignerConditionRule rule,
        out bool ruleResult)
    {
        ruleResult = false;
        var field = rule.Field?.Trim() ?? string.Empty;
        var parts = field.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !parts[0].Equals("CategoryDiscountPoints", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!TryGetPropertyInsensitive(root, parts[0], out var catObj) || catObj.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (TryGetPropertyInsensitive(catObj, parts[1], out _))
        {
            return false;
        }

        var valueStr = (rule.Value ?? string.Empty).Trim().Trim('"');
        var op = (rule.Operator ?? string.Empty).Trim();
        if (op is ">" or "<" or ">=" or "<=" or "==" or "!=")
        {
            if (!double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var numRight))
            {
                return false;
            }

            ruleResult = CompareNumeric(0, numRight, op);
            return true;
        }

        if (op is "include" or "notinclude")
        {
            ruleResult = CompareString(string.Empty, valueStr, op);
            return true;
        }

        return false;
    }

    private static bool TryGetPropertyInsensitive(JsonElement obj, string name, out JsonElement value)
    {
        value = default;
        if (obj.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (obj.TryGetProperty(name, out value))
        {
            return true;
        }

        foreach (var p in obj.EnumerateObject())
        {
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = p.Value;
                return true;
            }
        }

        return false;
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
        var trimmedLeft = (left ?? string.Empty).Trim().Trim('"');
        var trimmedRight = (right ?? string.Empty).Trim().Trim('"');
        if (trimmedRight.Contains(',') && (op == "==" || op == "!="))
        {
            var rightTokens = SplitCommaSeparatedConditionValues(trimmedRight);
            if (rightTokens.Count == 0)
            {
                return op == "!=";
            }

            var leftTokens = trimmedLeft.Contains(',')
                ? SplitCommaSeparatedConditionValues(trimmedLeft)
                : [trimmedLeft];

            var anyMatch = leftTokens.Any(l =>
                rightTokens.Any(r => string.Equals(l, r, StringComparison.OrdinalIgnoreCase)));

            return op == "==" ? anyMatch : !anyMatch;
        }

        var cmp = string.Compare(trimmedLeft, trimmedRight, StringComparison.OrdinalIgnoreCase);
        return op switch
        {
            "==" => cmp == 0,
            "!=" => cmp != 0,
            ">" => cmp > 0,
            "<" => cmp < 0,
            ">=" => cmp >= 0,
            "<=" => cmp <= 0,
            "include" => trimmedLeft.Contains(trimmedRight, StringComparison.OrdinalIgnoreCase),
            "notinclude" => !trimmedLeft.Contains(trimmedRight, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static List<string> SplitCommaSeparatedConditionValues(string right)
    {
        return right
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static s => s.Length > 0)
            .ToList();
    }
}
