using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

/// <summary>
/// 工作流任务授权快照ID
/// </summary>
public partial record WorkflowTaskAssignmentSnapshotId : IGuidStronglyTypedId
{
    /// <summary>
    /// 未分配标识（哨兵值）
    /// </summary>
    public static WorkflowTaskAssignmentSnapshotId Unassigned { get; } = new(Guid.Empty);
}

/// <summary>
/// 工作流任务授权快照。任务创建时固化谁可处理/查看以及授权来源。
/// </summary>
public class WorkflowTaskAssignmentSnapshot : Entity<WorkflowTaskAssignmentSnapshotId>
{
    protected WorkflowTaskAssignmentSnapshot()
    {
    }

    /// <summary>
    /// 任务ID
    /// </summary>
    public WorkflowTaskId WorkflowTaskId { get; private set; } = WorkflowTaskId.Unassigned;

    /// <summary>
    /// 处理人类型
    /// </summary>
    public AssigneeType AssigneeType { get; private set; }

    /// <summary>
    /// 处理人用户ID
    /// </summary>
    public UserId AssigneeUserId { get; private set; } = UserId.Unassigned;

    /// <summary>
    /// 处理人角色ID
    /// </summary>
    public RoleId AssigneeRoleId { get; private set; } = RoleId.Unassigned;

    /// <summary>
    /// 处理人显示名
    /// </summary>
    public string AssigneeDisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// 授权来源
    /// </summary>
    public WorkflowAssignmentSource AssignmentSource { get; private set; }

    /// <summary>
    /// 来源规则ID
    /// </summary>
    public string SourceRuleId { get; private set; } = string.Empty;

    /// <summary>
    /// 可见性模式
    /// </summary>
    public WorkflowTaskVisibilityMode VisibilityMode { get; private set; }

    /// <summary>
    /// 是否绕过常规数据权限过滤
    /// </summary>
    public bool BypassDataPermission { get; private set; }

    /// <summary>
    /// 发起部门范围模式
    /// </summary>
    public WorkflowTaskInitiatorDeptScopeMode InitiatorDeptScopeMode { get; private set; }

    /// <summary>
    /// 配置的发起部门范围 JSON
    /// </summary>
    public string InitiatorDeptScopeDeptIdsJson { get; private set; } = "[]";

    /// <summary>
    /// 创建原因
    /// </summary>
    public string CreatedReason { get; private set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    private WorkflowTaskAssignmentSnapshot(
        UserId assigneeUserId,
        string assigneeDisplayName,
        WorkflowAssignmentSource assignmentSource,
        string sourceRuleId,
        WorkflowTaskVisibilityMode visibilityMode,
        bool bypassDataPermission,
        WorkflowTaskInitiatorDeptScopeMode initiatorDeptScopeMode,
        string initiatorDeptScopeDeptIdsJson,
        string createdReason)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        AssigneeType = AssigneeType.User;
        AssigneeUserId = assigneeUserId;
        AssigneeRoleId = RoleId.Unassigned;
        AssigneeDisplayName = assigneeDisplayName;
        AssignmentSource = assignmentSource;
        SourceRuleId = sourceRuleId;
        VisibilityMode = visibilityMode;
        BypassDataPermission = bypassDataPermission;
        InitiatorDeptScopeMode = initiatorDeptScopeMode;
        InitiatorDeptScopeDeptIdsJson = string.IsNullOrWhiteSpace(initiatorDeptScopeDeptIdsJson)
            ? "[]"
            : initiatorDeptScopeDeptIdsJson;
        CreatedReason = createdReason;
    }

    /// <summary>
    /// 创建用户授权快照。任务ID由任务导航关系在持久化时维护。
    /// </summary>
    public static WorkflowTaskAssignmentSnapshot ForUser(
        UserId assigneeUserId,
        string assigneeDisplayName,
        WorkflowAssignmentSource assignmentSource,
        string sourceRuleId,
        WorkflowTaskVisibilityMode visibilityMode,
        bool bypassDataPermission,
        WorkflowTaskInitiatorDeptScopeMode initiatorDeptScopeMode,
        string initiatorDeptScopeDeptIdsJson,
        string createdReason)
    {
        return new WorkflowTaskAssignmentSnapshot(
            assigneeUserId,
            assigneeDisplayName,
            assignmentSource,
            sourceRuleId,
            visibilityMode,
            bypassDataPermission,
            initiatorDeptScopeMode,
            initiatorDeptScopeDeptIdsJson,
            createdReason);
    }

    /// <summary>
    /// 创建角色授权快照。任务ID由任务导航关系在持久化时维护。
    /// </summary>
    public static WorkflowTaskAssignmentSnapshot ForRole(
        RoleId assigneeRoleId,
        string assigneeDisplayName,
        WorkflowAssignmentSource assignmentSource,
        string sourceRuleId,
        WorkflowTaskVisibilityMode visibilityMode,
        bool bypassDataPermission,
        WorkflowTaskInitiatorDeptScopeMode initiatorDeptScopeMode,
        string initiatorDeptScopeDeptIdsJson,
        string createdReason)
    {
        var snapshot = new WorkflowTaskAssignmentSnapshot(
            UserId.Unassigned,
            assigneeDisplayName,
            assignmentSource,
            sourceRuleId,
            visibilityMode,
            bypassDataPermission,
            initiatorDeptScopeMode,
            initiatorDeptScopeDeptIdsJson,
            createdReason)
        {
            AssigneeType = AssigneeType.Role,
            AssigneeRoleId = assigneeRoleId
        };
        return snapshot;
    }

    /// <summary>
    /// 当前用户是否命中快照。
    /// </summary>
    public bool Matches(UserId userId, IReadOnlyCollection<RoleId> roleIds)
    {
        return AssigneeType switch
        {
            AssigneeType.User => AssigneeUserId == userId,
            AssigneeType.Role => roleIds.Contains(AssigneeRoleId),
            _ => false,
        };
    }
}

/// <summary>
/// 授权来源
/// </summary>
public enum WorkflowAssignmentSource
{
    /// <summary>
    /// 指定成员
    /// </summary>
    Member = 0,

    /// <summary>
    /// 指定角色
    /// </summary>
    Role = 1,

    /// <summary>
    /// 部门负责人
    /// </summary>
    DeptResponsibleUser = 2,

    /// <summary>
    /// 流程发起人
    /// </summary>
    InitiatorSelf = 3,

    /// <summary>
    /// 空审批人兜底策略
    /// </summary>
    EmptyApproverFallback = 4,

    /// <summary>
    /// 自审策略重定向
    /// </summary>
    SelfApprovalRedirect = 5,

    /// <summary>
    /// 业务变量解析
    /// </summary>
    BusinessVariable = 6,

    /// <summary>
    /// 转办产生
    /// </summary>
    Transferred = 7,

    /// <summary>
    /// 委托产生
    /// </summary>
    Delegated = 8,

    /// <summary>
    /// 退回重新生成
    /// </summary>
    Returned = 9,

    /// <summary>
    /// 订单合同签订公司负责人
    /// </summary>
    OrderContractSigningCompanyResponsibleUser = 10
}

/// <summary>
/// 任务可见性模式
/// </summary>
public enum WorkflowTaskVisibilityMode
{
    /// <summary>
    /// 明确指定用户
    /// </summary>
    ExplicitUser = 0,

    /// <summary>
    /// 按角色数据权限可见
    /// </summary>
    RoleDataPermission = 1,

    /// <summary>
    /// 绕过常规数据权限
    /// </summary>
    BypassDataPermission = 2,

    /// <summary>
    /// 角色数据权限叠加配置部门范围
    /// </summary>
    RoleDataPermissionPlusConfiguredDept = 3
}

/// <summary>
/// 工作流节点上的发起部门范围模式
/// </summary>
public enum WorkflowTaskInitiatorDeptScopeMode
{
    /// <summary>
    /// 使用处理人的数据权限覆盖发起部门
    /// </summary>
    DataPermission = 0,

    /// <summary>
    /// 全部发起部门
    /// </summary>
    All = 1,

    /// <summary>
    /// 指定部门及其下级
    /// </summary>
    SpecifiedDeptAndSub = 2
}
