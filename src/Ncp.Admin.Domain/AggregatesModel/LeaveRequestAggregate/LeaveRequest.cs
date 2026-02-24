using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents.LeaveEvents;

namespace Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;

/// <summary>
/// 请假申请ID（强类型ID）
/// </summary>
public partial record LeaveRequestId : IGuidStronglyTypedId;

/// <summary>
/// 请假申请聚合根
/// 年假/事假/病假/调休，与工作流审批集成
/// </summary>
public class LeaveRequest : Entity<LeaveRequestId>, IAggregateRoot
{
    protected LeaveRequest()
    {
    }

    /// <summary>
    /// 申请人ID
    /// </summary>
    public UserId ApplicantId { get; private set; } = default!;

    /// <summary>
    /// 申请人姓名（冗余）
    /// </summary>
    public string ApplicantName { get; private set; } = string.Empty;

    /// <summary>
    /// 请假类型
    /// </summary>
    public LeaveType LeaveType { get; private set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateOnly EndDate { get; private set; }

    /// <summary>
    /// 请假天数
    /// </summary>
    public decimal Days { get; private set; }

    /// <summary>
    /// 请假事由
    /// </summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// 状态
    /// </summary>
    public LeaveRequestStatus Status { get; private set; }

    /// <summary>
    /// 关联的工作流实例ID（提交审批后填充）
    /// </summary>
    public WorkflowInstanceId? WorkflowInstanceId { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建请假申请（草稿）
    /// </summary>
    public LeaveRequest(
        UserId applicantId,
        string applicantName,
        LeaveType leaveType,
        DateOnly startDate,
        DateOnly endDate,
        decimal days,
        string reason)
    {
        ApplicantId = applicantId;
        ApplicantName = applicantName;
        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        Days = days;
        Reason = reason ?? string.Empty;
        Status = LeaveRequestStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new LeaveRequestCreatedDomainEvent(this));
    }

    /// <summary>
    /// 提交审批（关联工作流实例后变为待审批）
    /// </summary>
    public void Submit(WorkflowInstanceId workflowInstanceId)
    {
        if (Status != LeaveRequestStatus.Draft)
        {
            throw new KnownException("只有草稿状态的请假单可以提交", ErrorCodes.LeaveRequestNotDraft);
        }

        WorkflowInstanceId = workflowInstanceId;
        Status = LeaveRequestStatus.Pending;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new LeaveRequestSubmittedDomainEvent(this));
    }

    /// <summary>
    /// 审批通过（工作流完成后由领域事件处理器调用）
    /// </summary>
    public void Approve()
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new KnownException("只有待审批状态的请假单可以审批通过", ErrorCodes.LeaveRequestNotPending);
        }

        Status = LeaveRequestStatus.Approved;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new LeaveRequestApprovedDomainEvent(this));
    }

    /// <summary>
    /// 审批驳回
    /// </summary>
    public void Reject()
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new KnownException("只有待审批状态的请假单可以驳回", ErrorCodes.LeaveRequestNotPending);
        }

        Status = LeaveRequestStatus.Rejected;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 撤销（仅草稿或待审批可撤销）
    /// </summary>
    public void Cancel()
    {
        if (Status != LeaveRequestStatus.Draft && Status != LeaveRequestStatus.Pending)
        {
            throw new KnownException("只有草稿或待审批的请假单可以撤销", ErrorCodes.LeaveRequestCannotCancel);
        }

        Status = LeaveRequestStatus.Cancelled;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 更新草稿内容（仅草稿可编辑）
    /// </summary>
    public void UpdateDraft(LeaveType leaveType, DateOnly startDate, DateOnly endDate, decimal days, string reason)
    {
        if (Status != LeaveRequestStatus.Draft)
        {
            throw new KnownException("只有草稿状态的请假单可以修改", ErrorCodes.LeaveRequestNotDraft);
        }

        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        Days = days;
        Reason = reason ?? string.Empty;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}

/// <summary>
/// 请假类型
/// </summary>
public enum LeaveType
{
    /// <summary>
    /// 年假
    /// </summary>
    Annual = 0,
    /// <summary>
    /// 事假
    /// </summary>
    Personal = 1,
    /// <summary>
    /// 病假
    /// </summary>
    Sick = 2,
    /// <summary>
    /// 调休
    /// </summary>
    Compensatory = 3,
}

/// <summary>
/// 请假单状态
/// </summary>
public enum LeaveRequestStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,
    /// <summary>
    /// 待审批
    /// </summary>
    Pending = 1,
    /// <summary>
    /// 已通过
    /// </summary>
    Approved = 2,
    /// <summary>
    /// 已驳回
    /// </summary>
    Rejected = 3,
    /// <summary>
    /// 已撤销
    /// </summary>
    Cancelled = 4,
}
