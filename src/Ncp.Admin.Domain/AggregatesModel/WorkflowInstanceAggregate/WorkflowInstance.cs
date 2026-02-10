using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

/// <summary>
/// 流程实例ID（强类型ID）
/// </summary>
public partial record WorkflowInstanceId : IGuidStronglyTypedId;

/// <summary>
/// 流程实例聚合根
/// 表示一个正在执行的工作流实例
/// </summary>
public class WorkflowInstance : Entity<WorkflowInstanceId>, IAggregateRoot
{
    protected WorkflowInstance()
    {
    }

    /// <summary>
    /// 关联的流程定义ID
    /// </summary>
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; } = default!;

    /// <summary>
    /// 流程定义名称（冗余存储）
    /// </summary>
    public string WorkflowDefinitionName { get; private set; } = string.Empty;

    /// <summary>
    /// 业务关联键（如订单号、请假单号等）
    /// </summary>
    public string BusinessKey { get; private set; } = string.Empty;

    /// <summary>
    /// 业务类型（如：LeaveRequest、PurchaseOrder等）
    /// </summary>
    public string BusinessType { get; private set; } = string.Empty;

    /// <summary>
    /// 流程标题
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// 发起人ID
    /// </summary>
    public UserId InitiatorId { get; private set; } = default!;

    /// <summary>
    /// 发起人姓名（冗余存储）
    /// </summary>
    public string InitiatorName { get; private set; } = string.Empty;

    /// <summary>
    /// 流程状态
    /// </summary>
    public WorkflowInstanceStatus Status { get; private set; } = WorkflowInstanceStatus.Running;

    /// <summary>
    /// 当前节点名称
    /// </summary>
    public string CurrentNodeName { get; private set; } = string.Empty;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// 流程变量JSON
    /// </summary>
    public string Variables { get; private set; } = "{}";

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>
    /// 业务执行失败原因（当 Status 为 Faulted 时使用）
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// 流程任务集合
    /// </summary>
    public virtual ICollection<WorkflowTask> Tasks { get; } = [];

    /// <summary>
    /// 创建流程实例
    /// </summary>
    public WorkflowInstance(
        WorkflowDefinitionId workflowDefinitionId,
        string workflowDefinitionName,
        string businessKey,
        string businessType,
        string title,
        UserId initiatorId,
        string initiatorName,
        string variables,
        string remark)
    {
        StartedAt = DateTimeOffset.UtcNow;
        WorkflowDefinitionId = workflowDefinitionId;
        WorkflowDefinitionName = workflowDefinitionName;
        BusinessKey = businessKey;
        BusinessType = businessType;
        Title = title;
        InitiatorId = initiatorId;
        InitiatorName = initiatorName;
        Status = WorkflowInstanceStatus.Running;
        Variables = variables;
        Remark = remark;

        AddDomainEvent(new WorkflowInstanceStartedDomainEvent(this));
    }

    /// <summary>
    /// 创建审批任务（指定用户）
    /// </summary>
    public WorkflowTask CreateTask(
        string nodeName,
        WorkflowTaskType taskType,
        UserId assigneeId,
        string assigneeName)
    {
        var task = new WorkflowTask(nodeName, taskType, assigneeId, assigneeName);
        Tasks.Add(task);
        CurrentNodeName = nodeName;

        AddDomainEvent(new WorkflowTaskCreatedDomainEvent(this, task));
        return task;
    }

    /// <summary>
    /// 创建审批任务（指定角色，一条记录，待办按角色 ID 查）
    /// </summary>
    public WorkflowTask CreateTaskForRole(
        string nodeName,
        WorkflowTaskType taskType,
        RoleId assigneeRoleId,
        string assigneeName)
    {
        var task = new WorkflowTask(nodeName, taskType, assigneeRoleId, assigneeName);
        Tasks.Add(task);
        CurrentNodeName = nodeName;

        AddDomainEvent(new WorkflowTaskCreatedDomainEvent(this, task));
        return task;
    }

    /// <summary>
    /// 审批通过任务
    /// </summary>
    public void ApproveTask(WorkflowTaskId taskId, UserId operatorId, string comment)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == taskId)
            ?? throw new KnownException("未找到该任务", ErrorCodes.WorkflowTaskNotFound);

        if (task.Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }

        task.Approve(comment);

        AddDomainEvent(new WorkflowTaskCompletedDomainEvent(this, task));
    }

    /// <summary>
    /// 驳回任务
    /// </summary>
    public void RejectTask(WorkflowTaskId taskId, UserId operatorId, string comment)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == taskId)
            ?? throw new KnownException("未找到该任务", ErrorCodes.WorkflowTaskNotFound);

        if (task.Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }

        task.Reject(comment);
        Status = WorkflowInstanceStatus.Rejected;
        CompletedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new WorkflowTaskCompletedDomainEvent(this, task));
        AddDomainEvent(new WorkflowInstanceRejectedDomainEvent(this));
    }

    /// <summary>
    /// 转办任务
    /// </summary>
    public void TransferTask(WorkflowTaskId taskId, UserId newAssigneeId, string newAssigneeName, string comment)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == taskId)
            ?? throw new KnownException("未找到该任务", ErrorCodes.WorkflowTaskNotFound);

        if (task.Status != WorkflowTaskStatus.Pending)
        {
            throw new KnownException("该任务已处理", ErrorCodes.WorkflowTaskAlreadyProcessed);
        }

        task.Transfer(comment);

        // 创建新任务分配给新的处理人
        var newTask = new WorkflowTask(task.NodeName, task.TaskType, newAssigneeId, newAssigneeName);
        Tasks.Add(newTask);

        AddDomainEvent(new WorkflowTaskCreatedDomainEvent(this, newTask));
    }

    /// <summary>
    /// 完成流程（所有节点通过）
    /// </summary>
    public void Complete()
    {
        if (Status != WorkflowInstanceStatus.Running)
        {
            throw new KnownException("流程未在运行中", ErrorCodes.WorkflowInstanceNotRunning);
        }

        Status = WorkflowInstanceStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new WorkflowInstanceCompletedDomainEvent(this));
    }

    /// <summary>
    /// 撤销流程（由发起人操作）
    /// </summary>
    public void Cancel(UserId operatorId)
    {
        if (Status != WorkflowInstanceStatus.Running)
        {
            throw new KnownException("流程未在运行中", ErrorCodes.WorkflowInstanceNotRunning);
        }

        if (InitiatorId != operatorId)
        {
            throw new KnownException("只有发起人可以撤销流程", ErrorCodes.WorkflowOnlyInitiatorCanCancel);
        }

        Status = WorkflowInstanceStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;

        // 将所有待办任务标记为已取消
        foreach (var task in Tasks.Where(t => t.Status == WorkflowTaskStatus.Pending))
        {
            task.Cancel();
        }

        AddDomainEvent(new WorkflowInstanceCompletedDomainEvent(this));
    }

    /// <summary>
    /// 挂起流程
    /// </summary>
    public void Suspend()
    {
        if (Status != WorkflowInstanceStatus.Running)
        {
            throw new KnownException("流程未在运行中", ErrorCodes.WorkflowInstanceNotRunning);
        }

        Status = WorkflowInstanceStatus.Suspended;
    }

    /// <summary>
    /// 恢复流程
    /// </summary>
    public void Resume()
    {
        if (Status != WorkflowInstanceStatus.Suspended)
        {
            throw new KnownException("流程未处于挂起状态", ErrorCodes.WorkflowInstanceNotSuspended);
        }

        Status = WorkflowInstanceStatus.Running;
    }

    /// <summary>
    /// 更新流程变量
    /// </summary>
    public void UpdateVariables(string variables)
    {
        Variables = variables;
    }

    /// <summary>
    /// 标记为异常（审批通过后业务执行失败时调用，仅允许对已完成状态的实例调用）
    /// </summary>
    public void MarkFaulted(string failureReason)
    {
        if (Status != WorkflowInstanceStatus.Completed)
        {
            throw new KnownException("仅能对已完成的流程标记业务执行异常", ErrorCodes.WorkflowInstanceNotRunning);
        }

        Status = WorkflowInstanceStatus.Faulted;
        FailureReason = failureReason;
    }
}

/// <summary>
/// 流程实例状态
/// </summary>
public enum WorkflowInstanceStatus
{
    /// <summary>
    /// 运行中
    /// </summary>
    Running = 0,

    /// <summary>
    /// 已挂起
    /// </summary>
    Suspended = 1,

    /// <summary>
    /// 已完成（审批通过）
    /// </summary>
    Completed = 2,

    /// <summary>
    /// 已驳回
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// 异常
    /// </summary>
    Faulted = 5
}
