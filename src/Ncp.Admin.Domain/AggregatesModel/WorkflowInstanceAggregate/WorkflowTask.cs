using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

/// <summary>
/// 工作流任务ID（强类型ID）
/// </summary>
public partial record WorkflowTaskId : IGuidStronglyTypedId
{
    /// <summary>
    /// 未分配标识（哨兵值）
    /// </summary>
    public static WorkflowTaskId Unassigned { get; } = new(Guid.Empty);
}

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
    public WorkflowInstanceId WorkflowInstanceId { get; private set; } = WorkflowInstanceId.Unassigned;

    /// <summary>
    /// 节点唯一标识（设计器 nodeKey，引擎追踪用）
    /// </summary>
    public string NodeKey { get; private set; } = string.Empty;

    /// <summary>
    /// 节点名称（展示用）
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
    /// 处理人用户ID（按角色分配任务时为哨兵 <c>UserId.Unassigned</c>）
    /// </summary>
    public UserId AssigneeId { get; private set; } = UserId.Unassigned;

    /// <summary>
    /// 处理人角色ID（按用户分配任务时为哨兵 <c>Guid.Empty</c>）
    /// </summary>
    public RoleId AssigneeRoleId { get; private set; } = RoleId.Unassigned;

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
    /// 任务扩展数据 JSON。用于保存退回上下文等不影响通用任务状态机的附加信息。
    /// </summary>
    public string ExtraDataJson { get; private set; } = "{}";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTimeOffset CompletedAt { get; private set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// 审批通过时的实际操作人（按角色派单时 <see cref="AssigneeId"/> 为 0，此处记录谁点的通过，供后续「相对上一节点」解析等使用）。
    /// </summary>
    public UserId CompletedByUserId { get; private set; } = UserId.Unassigned;

    /// <summary>
    /// 行版本号（框架自动处理并发检查）
    /// </summary>
    public RowVersion Version { get; private set; } = new RowVersion();

    /// <summary>
    /// 任务授权快照集合。任务创建时由聚合记录可处理人与授权来源。
    /// </summary>
    public virtual ICollection<WorkflowTaskAssignmentSnapshot> AssignmentSnapshots { get; } = [];

    /// <summary>
    /// 创建工作流任务（指定用户）
    /// </summary>
    internal WorkflowTask(string nodeKey, string nodeName, WorkflowTaskType taskType, UserId assigneeId, string assigneeName)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        NodeKey = nodeKey;
        NodeName = nodeName;
        TaskType = taskType;
        AssigneeType = AssigneeType.User;
        AssigneeId = assigneeId;
        AssigneeRoleId = RoleId.Unassigned;
        AssigneeName = assigneeName;
        Status = WorkflowTaskStatus.Pending;
    }

    /// <summary>
    /// 创建工作流任务（指定角色，一条记录，待办按角色查）
    /// </summary>
    internal WorkflowTask(string nodeKey, string nodeName, WorkflowTaskType taskType, RoleId assigneeRoleId, string assigneeName)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        NodeKey = nodeKey;
        NodeName = nodeName;
        TaskType = taskType;
        AssigneeType = AssigneeType.Role;
        AssigneeId = UserId.Unassigned;
        AssigneeRoleId = assigneeRoleId;
        AssigneeName = assigneeName;
        Status = WorkflowTaskStatus.Pending;
    }

    /// <summary>
    /// 记录任务创建时的授权快照。
    /// </summary>
    internal void AddAssignmentSnapshot(WorkflowTaskAssignmentSnapshot snapshot)
    {
        AssignmentSnapshots.Add(snapshot);
    }

    /// <summary>
    /// 审批通过
    /// </summary>
    /// <param name="comment">审批意见</param>
    /// <param name="completedByUserId">实际操作人（与指派用户一致或角色任务下点通过的用户）</param>
    public void Approve(string comment, UserId completedByUserId)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }
        Status = WorkflowTaskStatus.Approved;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
        CompletedByUserId = completedByUserId;
    }

    /// <summary>
    /// 驳回
    /// </summary>
    public void Reject(string comment)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }
        Status = WorkflowTaskStatus.Rejected;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 退回到上一审批节点。
    /// </summary>
    /// <param name="comment">退回说明。</param>
    /// <param name="completedByUserId">执行退回的实际操作人。</param>
    public void Return(string comment, UserId completedByUserId)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }

        if (TaskType != WorkflowTaskType.Approval)
        {
            throw new KnownException("只有审批任务可以退回", ErrorCodes.WorkflowTaskNotFound);
        }

        Status = WorkflowTaskStatus.Returned;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
        CompletedByUserId = completedByUserId;
    }

    /// <summary>
    /// 转办（当前任务标记为已转办）
    /// </summary>
    public void Transfer(string comment)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }
        Status = WorkflowTaskStatus.Transferred;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 取消
    /// </summary>
    public void Cancel()
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            return;
        }
        Status = WorkflowTaskStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 设置任务扩展数据。
    /// </summary>
    public void SetExtraDataJson(string extraDataJson)
    {
        ExtraDataJson = string.IsNullOrWhiteSpace(extraDataJson) ? "{}" : extraDataJson;
    }

    /// <summary>
    /// 标记抄送任务已读。
    /// </summary>
    public void MarkRead(string comment, UserId completedByUserId)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }

        if (TaskType != WorkflowTaskType.CarbonCopy)
        {
            throw new KnownException("只有抄送任务可以标记已读", ErrorCodes.WorkflowTaskNotFound);
        }

        Status = WorkflowTaskStatus.Read;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
        CompletedByUserId = completedByUserId;
    }

    /// <summary>
    /// 标记通知任务完成。
    /// </summary>
    public void CompleteNotice(string comment, UserId completedByUserId)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }

        if (TaskType != WorkflowTaskType.Notification)
        {
            throw new KnownException("只有通知任务可以标记完成", ErrorCodes.WorkflowTaskNotFound);
        }

        Status = WorkflowTaskStatus.Completed;
        Comment = comment;
        CompletedAt = DateTimeOffset.UtcNow;
        CompletedByUserId = completedByUserId;
    }

    /// <summary>
    /// 委托（当前任务标记为已委托）
    /// </summary>
    public void Delegate(string comment, string delegateToUserName)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }
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
    Delegated = 5,

    /// <summary>
    /// 已读
    /// </summary>
    Read = 6,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 7,

    /// <summary>
    /// 自动跳过
    /// </summary>
    AutoSkipped = 8,

    /// <summary>
    /// 已退回
    /// </summary>
    Returned = 9
}

/// <summary>
/// 处理人类型（指定用户 / 指定角色）
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
    Role = 1
}
