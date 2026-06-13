using System.Text.Json.Serialization;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 设计器条件分支中的单条规则（field op value）。
/// </summary>
public record DesignerConditionRule(
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("value")] string Value);

/// <summary>
/// 实例详情进度条：在变量解析后的单一条件路径上的发起人/审批/抄送步骤。
/// </summary>
public record WorkflowProgressStepItem(string Title, string? NodeKey);
