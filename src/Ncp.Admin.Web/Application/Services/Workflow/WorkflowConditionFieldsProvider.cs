namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 条件字段可选值（用于前端下拉，value 须与工作流 Variables JSON 中实际类型一致）。
/// </summary>
public record ConditionFieldOptionDto(string Value, string Label);

/// <summary>
/// 条件字段定义 DTO（按流程分类返回可用字段，供前端结构化条件表单使用）。
/// </summary>
/// <param name="Key">与 Variables JSON 属性名一致（PascalCase）。</param>
/// <param name="Label">展示名称。</param>
/// <param name="Type">number | string | boolean | enum | enumMulti | presence（presence 仅 为空/不为空，运算符固定 ==）。</param>
/// <param name="Options">有值时前端应使用下拉框而非自由输入。</param>
public record ConditionFieldDto(
    string Key,
    string Label,
    string Type,
    IReadOnlyList<ConditionFieldOptionDto>? Options = null);

/// <summary>
/// 按流程分类返回可用于条件分支的字段定义（供前端结构化条件表单使用）。
/// </summary>
public class WorkflowConditionFieldsProvider(WorkflowBusinessAdapterDispatcher businessAdapterDispatcher)
{
    /// <summary>
    /// 从已注册的业务适配器中获取指定流程分类的条件字段。
    /// </summary>
    public List<ConditionFieldDto> GetFields(string category)
    {
        return businessAdapterDispatcher.GetConditionFields(category).ToList();
    }
}
