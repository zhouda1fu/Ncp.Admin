using System.Text.Json.Serialization;

namespace Ncp.Admin.Web.Application.Services.Workflow.Graph;

/// <summary>
/// 发布时固化的流程运行图。
/// </summary>
public sealed class WorkflowGraph
{
    /// <summary>
    /// 开始节点ID。
    /// </summary>
    [JsonPropertyName("startNodeId")]
    public string StartNodeId { get; set; } = string.Empty;

    /// <summary>
    /// 运行图节点列表。
    /// </summary>
    [JsonPropertyName("nodes")]
    public List<WorkflowGraphNode> Nodes { get; set; } = [];

    /// <summary>
    /// 没有产生任何任务时，是否允许流程自动完成。
    /// </summary>
    [JsonPropertyName("allowAutoCompleteWithoutTasks")]
    public bool AllowAutoCompleteWithoutTasks { get; set; }
}

/// <summary>
/// 流程运行图节点。
/// </summary>
public sealed class WorkflowGraphNode
{
    /// <summary>节点ID。</summary>
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; } = string.Empty;

    /// <summary>节点名称。</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>节点类型。</summary>
    [JsonPropertyName("type")]
    public WorkflowGraphNodeType Type { get; set; }

    /// <summary>下一节点ID。</summary>
    [JsonPropertyName("nextNodeId")]
    public string? NextNodeId { get; set; }

    /// <summary>审批模式。</summary>
    [JsonPropertyName("approvalMode")]
    public WorkflowGraphApprovalMode ApprovalMode { get; set; }

    /// <summary>审批人规则。</summary>
    [JsonPropertyName("assigneeRules")]
    public List<WorkflowGraphAssigneeRule> AssigneeRules { get; set; } = [];

    /// <summary>抄送人规则。</summary>
    [JsonPropertyName("copyRules")]
    public List<WorkflowGraphAssigneeRule> CopyRules { get; set; } = [];

    /// <summary>审批人为空策略。</summary>
    [JsonPropertyName("emptyApproverPolicy")]
    public WorkflowGraphEmptyApproverPolicy EmptyApproverPolicy { get; set; } = new();

    /// <summary>自审策略。</summary>
    [JsonPropertyName("selfApprovalPolicy")]
    public WorkflowGraphSelfApprovalPolicy SelfApprovalPolicy { get; set; } = new();

    /// <summary>条件分支。</summary>
    [JsonPropertyName("branches")]
    public List<WorkflowGraphConditionBranch> Branches { get; set; } = [];

    /// <summary>条件分支汇聚节点ID。</summary>
    [JsonPropertyName("mergeNodeId")]
    public string? MergeNodeId { get; set; }

    /// <summary>业务扩展 JSON。</summary>
    [JsonPropertyName("extensionsJson")]
    public string ExtensionsJson { get; set; } = "{}";
}

/// <summary>
/// 运行期人员分配规则。
/// </summary>
public sealed class WorkflowGraphAssigneeRule
{
    /// <summary>规则ID。</summary>
    [JsonPropertyName("ruleId")]
    public string RuleId { get; set; } = string.Empty;

    /// <summary>人员来源。</summary>
    [JsonPropertyName("source")]
    public WorkflowGraphAssigneeSource Source { get; set; }

    /// <summary>指定用户。</summary>
    [JsonPropertyName("users")]
    public List<WorkflowGraphOption> Users { get; set; } = [];

    /// <summary>指定角色。</summary>
    [JsonPropertyName("roles")]
    public List<WorkflowGraphOption> Roles { get; set; } = [];

    /// <summary>指定部门。</summary>
    [JsonPropertyName("depts")]
    public List<WorkflowGraphOption> Depts { get; set; } = [];

    /// <summary>部门负责人层级。</summary>
    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;

    /// <summary>排除的指定用户。</summary>
    [JsonPropertyName("excludeUsers")]
    public List<WorkflowGraphOption> ExcludeUsers { get; set; } = [];

    /// <summary>额外追加的指定用户。</summary>
    [JsonPropertyName("extraUsers")]
    public List<WorkflowGraphOption> ExtraUsers { get; set; } = [];

    /// <summary>发起部门范围模式。</summary>
    [JsonPropertyName("initiatorDeptScopeMode")]
    public WorkflowGraphInitiatorDeptScopeMode InitiatorDeptScopeMode { get; set; }

    /// <summary>指定发起部门范围。</summary>
    [JsonPropertyName("initiatorDeptScopeDepts")]
    public List<WorkflowGraphOption> InitiatorDeptScopeDepts { get; set; } = [];
}

/// <summary>
/// 运行图选项。
/// </summary>
public sealed record WorkflowGraphOption(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);

/// <summary>
/// 审批人为空策略。
/// </summary>
public sealed class WorkflowGraphEmptyApproverPolicy
{
    /// <summary>策略模式。</summary>
    [JsonPropertyName("mode")]
    public WorkflowGraphEmptyApproverPolicyMode Mode { get; set; } = WorkflowGraphEmptyApproverPolicyMode.AutoPass;

    /// <summary>指定处理人。</summary>
    [JsonPropertyName("users")]
    public List<WorkflowGraphOption> Users { get; set; } = [];
}

/// <summary>
/// 自审策略。
/// </summary>
public sealed class WorkflowGraphSelfApprovalPolicy
{
    /// <summary>策略模式。</summary>
    [JsonPropertyName("mode")]
    public WorkflowGraphSelfApprovalPolicyMode Mode { get; set; } = WorkflowGraphSelfApprovalPolicyMode.Allow;
}

/// <summary>
/// 运行图条件分支。
/// </summary>
public sealed class WorkflowGraphConditionBranch
{
    /// <summary>分支ID。</summary>
    [JsonPropertyName("branchId")]
    public string BranchId { get; set; } = string.Empty;

    /// <summary>分支名称。</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>优先级。</summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    /// <summary>条件组。</summary>
    [JsonPropertyName("conditionGroups")]
    public List<List<DesignerConditionRule>> ConditionGroups { get; set; } = [];

    /// <summary>首个节点ID。</summary>
    [JsonPropertyName("firstNodeId")]
    public string? FirstNodeId { get; set; }

    /// <summary>是否为兜底分支。</summary>
    [JsonPropertyName("isFallback")]
    public bool IsFallback { get; set; }
}

/// <summary>
/// 运行图节点类型。
/// </summary>
public enum WorkflowGraphNodeType
{
    Start = 0,
    Approval = 1,
    CarbonCopy = 2,
    ConditionRoute = 3,
    End = 4,
    BusinessExtension = 5
}

/// <summary>
/// 审批模式。
/// </summary>
public enum WorkflowGraphApprovalMode
{
    Sequential = 0,
    All = 1,
    Any = 2
}

/// <summary>
/// 人员来源。
/// </summary>
public enum WorkflowGraphAssigneeSource
{
    /// <summary>指定成员。</summary>
    Member = 0,

    /// <summary>指定角色。</summary>
    Role = 1,

    /// <summary>指定层级部门负责人。</summary>
    DeptResponsibleUser = 2,

    /// <summary>流程发起人。</summary>
    Initiator = 3,

    /// <summary>业务变量。</summary>
    BusinessVariable = 4,

    /// <summary>部门负责人链。</summary>
    DeptResponsibleUserChain = 5,

    /// <summary>订单合同签订公司负责人。</summary>
    OrderContractSigningCompanyResponsibleUser = 6
}

/// <summary>
/// 发起部门范围模式。
/// </summary>
public enum WorkflowGraphInitiatorDeptScopeMode
{
    DataPermission = 0,
    All = 1,
    SpecifiedDeptAndSub = 2
}

/// <summary>
/// 审批人为空策略模式。
/// </summary>
public enum WorkflowGraphEmptyApproverPolicyMode
{
    AutoPass = 0,
    SpecifiedMembers = 1,
    WorkflowAdmin = 2
}

/// <summary>
/// 自审策略模式。
/// </summary>
public enum WorkflowGraphSelfApprovalPolicyMode
{
    Allow = 0,
    AutoSkip = 1,
    DirectResponsibleUser = 2,
    DeptResponsibleUser = 3
}
