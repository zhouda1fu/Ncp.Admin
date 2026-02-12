using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

/// <summary>
/// 工作流任务ID（强类型ID）
/// </summary>
public partial record WorkflowTaskId : IGuidStronglyTypedId;

/// <summary>
/// 工作流任务
/// 表示流程中某个节点的具体处理任务
/// </summary>
public class WorkflowTask : Entity<WorkflowTaskId>
{
    protected WorkflowTask()
    {
    }

    /// <summary>
    /// 关联的流程实例ID
    /// </summary>
    public WorkflowInstanceId WorkflowInstanceId { get; private set; } = default!;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; private set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    public WorkflowTaskType TaskType { get; private set; }

    /// <summary>
    /// 处理人类型（指定用户 / 指定角色）
    /// </summary>
    public AssigneeType AssigneeType { get; private set; }

    /// <summary>
    /// 处理人用户ID（指定用户时有值）
    /// </summary>
    public UserId? AssigneeId { get; private set; }

    /// <summary>
    /// 处理人角色ID（指定角色时有值）
    /// </summary>
    public RoleId? AssigneeRoleId { get; private set; }

    /// <summary>
    /// 处理人姓名/角色名（冗余存储，用于展示）
    /// </summary>
    public string AssigneeName { get; private set; } = string.Empty;

    /// <summary>
    /// 任务状态
    /// </summary>
    public WorkflowTaskStatus Status { get; private set; } = WorkflowTaskStatus.Pending;

    /// <summary>
    /// 审批意见
    /// </summary>
    public string Comment { get; private set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// 行版本号（框架自动处理并发检查）
    /// </summary>
    public RowVersion Version { get; private set; } = new RowVersion();

    /// <summary>
    /// 创建工作流任务（指定用户）
    /// </summary>
    public WorkflowTask(string nodeName, WorkflowTaskType taskType, UserId assigneeId, string assigneeName)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        NodeName = nodeName;
        TaskType = taskType;
        AssigneeType = AssigneeType.User;
        AssigneeId = assigneeId;
        AssigneeRoleId = null;
        AssigneeName = assigneeName;
        Status = WorkflowTaskStatus.Pending;
    }

    /// <summary>
    /// 创建工作流任务（指定角色，一条记录，待办按角色查）
    /// </summary>
    public WorkflowTask(string nodeName, WorkflowTaskType taskType, RoleId assigneeRoleId, string assigneeName)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        NodeName = nodeName;
        TaskType = taskType;
        AssigneeType = AssigneeType.Role;
        AssigneeId = null;
        AssigneeRoleId = assigneeRoleId;
        AssigneeName = assigneeName;
        Status = WorkflowTaskStatus.Pending;
    }

    /// <summary>
    /// 审批通过
    /// </summary>
    public void Approve(string comment)
    {
        Status = WorkflowTaskStatus.Approved;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 驳回
    /// </summary>
    public void Reject(string comment)
    {
        Status = WorkflowTaskStatus.Rejected;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 转办（当前任务标记为已转办）
    /// </summary>
    public void Transfer(string comment)
    {
        Status = WorkflowTaskStatus.Transferred;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 取消
    /// </summary>
    public void Cancel()
    {
        Status = WorkflowTaskStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 委托（当前任务标记为已委托）
    /// </summary>
    public void Delegate(string comment, string delegateToUserName)
    {
        Status = WorkflowTaskStatus.Delegated;
        Comment = $"已委托给 {delegateToUserName}。备注：{comment}";
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// 任务类型
/// </summary>
public enum WorkflowTaskType
{
    /// <summary>
    /// 审批
    /// </summary>
    Approval = 0,

    /// <summary>
    /// 通知
    /// </summary>
    Notification = 1,

    /// <summary>
    /// 抄送
    /// </summary>
    CarbonCopy = 2
}

/// <summary>
/// 任务状态
/// </summary>
public enum WorkflowTaskStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已通过
    /// </summary>
    Approved = 1,

    /// <summary>
    /// 已驳回
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// 已转办
    /// </summary>
    Transferred = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// 已委托
    /// </summary>
    Delegated = 5
}
