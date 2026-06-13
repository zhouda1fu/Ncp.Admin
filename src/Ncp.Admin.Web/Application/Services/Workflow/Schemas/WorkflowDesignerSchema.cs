using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ncp.Admin.Web.Application.Services.Workflow.Schemas;

/// <summary>
/// 工作流设计器保存到后端的当前 JSON 契约。
/// </summary>
public sealed class WorkflowDesignerSchema
{
    /// <summary>
    /// 开始节点ID。
    /// </summary>
    [JsonPropertyName("startNodeId")]
    public string StartNodeId { get; set; } = string.Empty;

    /// <summary>
    /// 流程中的全部节点。
    /// </summary>
    [JsonPropertyName("nodes")]
    public List<WorkflowDesignerNode> Nodes { get; set; } = [];

    /// <summary>
    /// 没有产生任何任务时，是否允许流程自动完成。
    /// </summary>
    [JsonPropertyName("allowAutoCompleteWithoutTasks")]
    public bool AllowAutoCompleteWithoutTasks { get; set; }
}

/// <summary>
/// 设计器节点。
/// </summary>
public sealed class WorkflowDesignerNode
{
    /// <summary>
    /// 节点ID，也是运行期追踪节点的稳定 key。
    /// </summary>
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点展示名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 节点类型。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = WorkflowDesignerNodeTypes.Start;

    /// <summary>
    /// 当前节点完成后的下一节点ID。
    /// </summary>
    [JsonPropertyName("nextNodeId")]
    public string? NextNodeId { get; set; }

    /// <summary>
    /// 审批模式：依次、会签、或签。
    /// </summary>
    [JsonPropertyName("approvalMode")]
    public string? ApprovalMode { get; set; }

    /// <summary>
    /// 审批节点的审批人规则。
    /// </summary>
    [JsonPropertyName("assigneeRules")]
    public List<WorkflowAssigneeRule> AssigneeRules { get; set; } = [];

    /// <summary>
    /// 抄送节点的接收人规则。
    /// </summary>
    [JsonPropertyName("copyRules")]
    public List<WorkflowAssigneeRule> CopyRules { get; set; } = [];

    /// <summary>
    /// 审批人为空时的处理策略。
    /// </summary>
    [JsonPropertyName("emptyApproverPolicy")]
    public WorkflowEmptyApproverPolicySchema? EmptyApproverPolicy { get; set; }

    /// <summary>
    /// 发起人与审批人相同时的处理策略。
    /// </summary>
    [JsonPropertyName("selfApprovalPolicy")]
    public WorkflowSelfApprovalPolicySchema? SelfApprovalPolicy { get; set; }

    /// <summary>
    /// 条件路由节点的分支。
    /// </summary>
    [JsonPropertyName("branches")]
    public List<WorkflowConditionBranchSchema> Branches { get; set; } = [];

    /// <summary>
    /// 条件路由分支汇聚后的节点ID。
    /// </summary>
    [JsonPropertyName("mergeNodeId")]
    public string? MergeNodeId { get; set; }

    /// <summary>
    /// 业务扩展配置，按业务域分组保存。
    /// </summary>
    [JsonPropertyName("extensions")]
    public Dictionary<string, JsonElement>? Extensions { get; set; }
}

/// <summary>
/// 审批人或抄送人选择规则。
/// </summary>
public sealed class WorkflowAssigneeRule
{
    /// <summary>
    /// 规则ID，便于后续审计和问题定位。
    /// </summary>
    [JsonPropertyName("ruleId")]
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// 人员来源。
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = WorkflowAssigneeSources.Member;

    /// <summary>
    /// 指定成员。
    /// </summary>
    [JsonPropertyName("users")]
    public List<WorkflowDesignerOption> Users { get; set; } = [];

    /// <summary>
    /// 指定角色。
    /// </summary>
    [JsonPropertyName("roles")]
    public List<WorkflowDesignerOption> Roles { get; set; } = [];

    /// <summary>
    /// 指定部门。
    /// </summary>
    [JsonPropertyName("depts")]
    public List<WorkflowDesignerOption> Depts { get; set; } = [];

    /// <summary>
    /// 部门负责人层级。
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;

    /// <summary>
    /// 排除的指定成员（部门负责人链规则使用）。
    /// </summary>
    [JsonPropertyName("excludeUsers")]
    public List<WorkflowDesignerOption> ExcludeUsers { get; set; } = [];

    /// <summary>
    /// 额外追加的指定成员（部门负责人链规则使用）。
    /// </summary>
    [JsonPropertyName("extraUsers")]
    public List<WorkflowDesignerOption> ExtraUsers { get; set; } = [];

    /// <summary>
    /// 支持发起部门范围的人员来源规则下限定发起部门范围的策略。
    /// </summary>
    [JsonPropertyName("initiatorDeptScope")]
    public WorkflowInitiatorDeptScopeSchema? InitiatorDeptScope { get; set; }
}

/// <summary>
/// 设计器选项。
/// </summary>
public sealed record WorkflowDesignerOption(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);

/// <summary>
/// 发起部门范围配置。
/// </summary>
public sealed class WorkflowInitiatorDeptScopeSchema
{
    /// <summary>
    /// 范围模式。
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = WorkflowInitiatorDeptScopeNames.DataPermission;

    /// <summary>
    /// 指定部门列表。
    /// </summary>
    [JsonPropertyName("depts")]
    public List<WorkflowDesignerOption> Depts { get; set; } = [];
}

/// <summary>
/// 审批人为空时的策略配置。
/// </summary>
public sealed class WorkflowEmptyApproverPolicySchema
{
    /// <summary>
    /// 策略模式。
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = WorkflowEmptyApproverPolicyNames.AutoPass;

    /// <summary>
    /// 指定处理人列表。
    /// </summary>
    [JsonPropertyName("users")]
    public List<WorkflowDesignerOption> Users { get; set; } = [];
}

/// <summary>
/// 发起人与审批人相同时的策略配置。
/// </summary>
public sealed class WorkflowSelfApprovalPolicySchema
{
    /// <summary>
    /// 策略模式。
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = WorkflowSelfApprovalPolicyNames.Allow;
}

/// <summary>
/// 条件路由分支配置。
/// </summary>
public sealed class WorkflowConditionBranchSchema
{
    /// <summary>
    /// 分支ID。
    /// </summary>
    [JsonPropertyName("branchId")]
    public string BranchId { get; set; } = string.Empty;

    /// <summary>
    /// 分支名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分支优先级，数字越小越先判断。
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// 条件组；外层为或，内层为且。
    /// </summary>
    [JsonPropertyName("conditionGroups")]
    public List<List<DesignerConditionRule>> ConditionGroups { get; set; } = [];

    /// <summary>
    /// 分支命中后的首个节点ID。
    /// </summary>
    [JsonPropertyName("firstNodeId")]
    public string? FirstNodeId { get; set; }

    /// <summary>
    /// 是否为兜底分支。
    /// </summary>
    [JsonPropertyName("isFallback")]
    public bool IsFallback { get; set; }
}

/// <summary>
/// 设计器节点类型名称。
/// </summary>
public static class WorkflowDesignerNodeTypes
{
    public const string Start = "start";
    public const string Approval = "approval";
    public const string CarbonCopy = "carbonCopy";
    public const string ConditionRoute = "conditionRoute";
    public const string End = "end";
    public const string BusinessExtension = "businessExtension";
}

public static class WorkflowApprovalModeNames
{
    public const string Sequential = "sequential";
    public const string All = "all";
    public const string Any = "any";
}

public static class WorkflowAssigneeSources
{
    /// <summary>指定成员。</summary>
    public const string Member = "member";

    /// <summary>指定角色。</summary>
    public const string Role = "role";

    /// <summary>指定层级部门负责人。</summary>
    public const string DeptResponsibleUser = "deptResponsibleUser";

    /// <summary>部门负责人链。</summary>
    public const string DeptResponsibleUserChain = "deptResponsibleUserChain";

    /// <summary>流程发起人。</summary>
    public const string Initiator = "initiator";

    /// <summary>业务变量。</summary>
    public const string BusinessVariable = "businessVariable";

    /// <summary>订单合同签订公司负责人。</summary>
    public const string OrderContractSigningCompanyResponsibleUser = "orderContractSigningCompanyResponsibleUser";
}

public static class WorkflowInitiatorDeptScopeNames
{
    public const string DataPermission = "dataPermission";
    public const string All = "all";
    public const string SpecifiedDeptAndSub = "specifiedDeptAndSub";
}

public static class WorkflowEmptyApproverPolicyNames
{
    public const string AutoPass = "autoPass";
    public const string SpecifiedMembers = "specifiedMembers";
    public const string WorkflowAdmin = "workflowAdmin";
}

public static class WorkflowSelfApprovalPolicyNames
{
    public const string Allow = "allow";
    public const string AutoSkip = "autoSkip";
    public const string DirectResponsibleUser = "directResponsibleUser";
    public const string DeptResponsibleUser = "deptResponsibleUser";
}
