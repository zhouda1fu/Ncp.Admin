using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 根据流程变量 JSON 解析条件表达式，得到用于分支判断的委托。
/// 表达式格式：key op value，如 amount &gt; 1000、status == "approved"
/// 支持运算符：&gt;, &lt;, &gt;=, &lt;=, ==, !=
/// </summary>
public static class WorkflowConditionEvaluator
{
    private static readonly Regex ExpressionRegex = new(
        @"^\s*(\w+)\s*(>=|<=|==|!=|>|<)\s*(.+)\s*$",
        RegexOptions.Compiled);

    /// <summary>
    /// 根据变量 JSON 构建条件求值函数。表达式为空时返回 false。
    /// </summary>
    public static Func<string, bool> CreateEvaluator(string? variablesJson)
    {
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

        return expression => Evaluate(root, expression);
    }

    private static bool Evaluate(JsonElement? variables, string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        var match = ExpressionRegex.Match(expression.Trim());
        if (!match.Success)
            return false;

        var key = match.Groups[1].Value;
        var op = match.Groups[2].Value;
        var valueStr = match.Groups[3].Value.Trim();

        if (!variables.HasValue || !variables.Value.TryGetProperty(key, out var prop))
            return false;

        // 解析比较值：数字或带引号的字符串
        valueStr = valueStr.Trim('"');
        if (prop.ValueKind == JsonValueKind.Number)
        {
            if (!double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var numRight))
                return false;
            double left = prop.GetDouble();
            return CompareNumeric(left, numRight, op);
        }

        var leftStr = prop.ValueKind == JsonValueKind.String ? prop.GetString() ?? "" : prop.GetRawText();
        return CompareString(leftStr, valueStr, op);
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
        var cmp = string.Compare(left, right, StringComparison.Ordinal);
        return op switch
        {
            "==" => cmp == 0,
            "!=" => cmp != 0,
            ">" => cmp > 0,
            "<" => cmp < 0,
            ">=" => cmp >= 0,
            "<=" => cmp <= 0,
            _ => false
        };
    }
}
