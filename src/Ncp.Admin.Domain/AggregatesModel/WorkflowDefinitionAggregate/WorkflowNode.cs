namespace Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

/// <summary>
/// 流程节点ID（强类型ID）
/// </summary>
public partial record WorkflowNodeId : IGuidStronglyTypedId;

/// <summary>
/// 流程节点
/// 定义了流程中每个审批/处理节点的信息
/// </summary>
public class WorkflowNode : Entity<WorkflowNodeId>
{
    protected WorkflowNode()
    {
    }

    /// <summary>
    /// 所属流程定义ID
    /// </summary>
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; } = default!;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; private set; } = string.Empty;

    /// <summary>
    /// 节点类型
    /// </summary>
    public WorkflowNodeType NodeType { get; private set; }

    /// <summary>
    /// 审批人类型（指定用户、角色、部门主管等）
    /// </summary>
    public AssigneeType AssigneeType { get; private set; }

    /// <summary>
    /// 审批人值（用户ID、角色ID等，根据AssigneeType而定）
    /// </summary>
    public string AssigneeValue { get; private set; } = string.Empty;

    /// <summary>
    /// 节点排序
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 节点描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 审批方式（或签=任一同意即通过, 会签=所有人同意才通过）
    /// </summary>
    public ApprovalMode ApprovalMode { get; private set; } = ApprovalMode.OrSign;

    /// <summary>
    /// 条件表达式（仅条件分支节点使用），如：amount &gt; 1000 或 status == "approved"
    /// </summary>
    public string ConditionExpression { get; private set; } = string.Empty;

    /// <summary>
    /// 条件为真时跳转的节点名称（仅条件分支节点使用）
    /// </summary>
    public string TrueNextNodeName { get; private set; } = string.Empty;

    /// <summary>
    /// 条件为假时跳转的节点名称（仅条件分支节点使用）
    /// </summary>
    public string FalseNextNodeName { get; private set; } = string.Empty;

    /// <summary>
    /// 创建流程节点
    /// </summary>
    public WorkflowNode(string nodeName, WorkflowNodeType nodeType, AssigneeType assigneeType, string assigneeValue, int sortOrder, string description, ApprovalMode approvalMode = ApprovalMode.OrSign, string? conditionExpression = null, string? trueNextNodeName = null, string? falseNextNodeName = null)
    {
        NodeName = nodeName;
        NodeType = nodeType;
        AssigneeType = assigneeType;
        AssigneeValue = assigneeValue;
        SortOrder = sortOrder;
        Description = description;
        ApprovalMode = approvalMode;
        ConditionExpression = conditionExpression ?? string.Empty;
        TrueNextNodeName = trueNextNodeName ?? string.Empty;
        FalseNextNodeName = falseNextNodeName ?? string.Empty;
    }
}

/// <summary>
/// 流程节点类型
/// </summary>
public enum WorkflowNodeType
{
    /// <summary>
    /// 开始节点
    /// </summary>
    Start = 0,

    /// <summary>
    /// 审批节点
    /// </summary>
    Approval = 1,

    /// <summary>
    /// 抄送节点
    /// </summary>
    CarbonCopy = 2,

    /// <summary>
    /// 通知节点
    /// </summary>
    Notification = 3,

    /// <summary>
    /// 条件分支节点
    /// </summary>
    Condition = 4,

    /// <summary>
    /// 结束节点
    /// </summary>
    End = 5
}

/// <summary>
/// 审批人类型
/// </summary>
public enum AssigneeType
{
    /// <summary>
    /// 指定用户
    /// </summary>
    User = 0,

    /// <summary>
    /// 指定角色
    /// </summary>
    Role = 1,

    /// <summary>
    /// 部门主管
    /// </summary>
    DeptManager = 2,

    /// <summary>
    /// 发起人自选
    /// </summary>
    InitiatorSelect = 3
}

/// <summary>
/// 审批方式
/// </summary>
public enum ApprovalMode
{
    /// <summary>
    /// 或签：任一审批人同意即通过
    /// </summary>
    OrSign = 0,

    /// <summary>
    /// 会签：所有审批人必须同意才通过
    /// </summary>
    CounterSign = 1,

    /// <summary>
    /// 依次审批：按顺序逐一审批
    /// </summary>
    Sequential = 2
}
