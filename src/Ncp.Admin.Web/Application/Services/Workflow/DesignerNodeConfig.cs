using System.Text.Json.Serialization;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 设计器节点中的审批人/角色项（id, name）
/// </summary>
public record DesignerAssignee(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);

/// <summary>
/// 设计器条件分支中的单条规则（field op value）
/// </summary>
public record DesignerConditionRule(
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("value")] string Value);

/// <summary>
/// 与前端流程设计器 JSON 结构对应的节点配置。
/// type: 0=发起人, 1=审批人, 2=抄送人, 3=条件分支项, 4=条件路由
/// setType (审批人): 1=指定成员, 2=主管, 3=角色, 4=发起人自选, 5=发起人自己, 7=连续多级主管
/// examineMode (审批人): 1=依次审批, 2=会签, 3=或签
/// </summary>
public class DesignerNodeConfig
{
    [JsonPropertyName("nodeName")]
    public string NodeName { get; set; } = string.Empty;

    [JsonPropertyName("nodeKey")]
    public string NodeKey { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("setType")]
    public int SetType { get; set; }

    [JsonPropertyName("nodeAssigneeList")]
    public List<DesignerAssignee>? NodeAssigneeList { get; set; }

    [JsonPropertyName("examineLevel")]
    public int ExamineLevel { get; set; }

    [JsonPropertyName("directorLevel")]
    public int DirectorLevel { get; set; }

    [JsonPropertyName("directorMode")]
    public int DirectorMode { get; set; }

    [JsonPropertyName("selectMode")]
    public int SelectMode { get; set; }

    [JsonPropertyName("termAuto")]
    public bool TermAuto { get; set; }

    [JsonPropertyName("term")]
    public int Term { get; set; }

    [JsonPropertyName("termMode")]
    public int TermMode { get; set; }

    [JsonPropertyName("examineMode")]
    public int ExamineMode { get; set; }

    [JsonPropertyName("userSelectFlag")]
    public bool UserSelectFlag { get; set; }

    [JsonPropertyName("conditionMode")]
    public int ConditionMode { get; set; }

    /// <summary>
    /// 条件列表：外层 List 为 OR，内层 List 为 AND。内层元素为 { field, operator, value }。
    /// </summary>
    [JsonPropertyName("conditionList")]
    public List<List<DesignerConditionRule>>? ConditionList { get; set; }

    [JsonPropertyName("priorityLevel")]
    public int PriorityLevel { get; set; }

    [JsonPropertyName("childNode")]
    public DesignerNodeConfig? ChildNode { get; set; }

    [JsonPropertyName("conditionNodes")]
    public List<DesignerNodeConfig>? ConditionNodes { get; set; }

    /// <summary>
    /// type=1 为审批节点
    /// </summary>
    public bool IsApprovalNode => Type == 1;

    /// <summary>
    /// examineMode: 2=会签
    /// </summary>
    public bool IsCounterSign => ExamineMode == 2;

    /// <summary>
    /// examineMode: 3=或签（任一人通过即可，其余待办应取消）
    /// </summary>
    public bool IsOrSign => ExamineMode == 3;

    /// <summary>
    /// 依次审批：examineMode 为 0（未显式配置）或 1；与会签(2)、或签(3)互斥。
    /// </summary>
    public bool IsSequentialApproval => Type == 1 && ExamineMode != 2 && ExamineMode != 3;
}

/// <summary>
/// 实例详情进度条：在变量解析后的单一条件路径上的发起人/审批/抄送步骤。
/// </summary>
public record WorkflowProgressStepItem(string Title, string? NodeKey);
