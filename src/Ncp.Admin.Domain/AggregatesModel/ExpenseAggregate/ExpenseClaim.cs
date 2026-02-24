using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;

/// <summary>
/// 报销单ID（强类型ID）
/// </summary>
public partial record ExpenseClaimId : IGuidStronglyTypedId;

/// <summary>
/// 报销单状态
/// </summary>
public enum ExpenseClaimStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,
    /// <summary>
    /// 已提交
    /// </summary>
    Submitted = 1,
    /// <summary>
    /// 已通过
    /// </summary>
    Approved = 2,
    /// <summary>
    /// 已驳回
    /// </summary>
    Rejected = 3,
}

/// <summary>
/// 报销单聚合根，包含多条报销明细，支持草稿编辑、提交与审批
/// </summary>
public class ExpenseClaim : Entity<ExpenseClaimId>, IAggregateRoot
{
    private readonly List<ExpenseItem> _items = [];

    protected ExpenseClaim() { }

    /// <summary>
    /// 申请人用户ID
    /// </summary>
    public UserId ApplicantId { get; private set; } = default!;
    /// <summary>
    /// 申请人姓名（冗余）
    /// </summary>
    public string ApplicantName { get; private set; } = string.Empty;
    /// <summary>
    /// 报销总金额（由明细汇总）
    /// </summary>
    public decimal TotalAmount { get; private set; }
    /// <summary>
    /// 报销单状态
    /// </summary>
    public ExpenseClaimStatus Status { get; private set; }
    /// <summary>
    /// 关联的工作流实例ID（提交审批后可选填充）
    /// </summary>
    public WorkflowInstanceId? WorkflowInstanceId { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 报销明细列表（只读）
    /// </summary>
    public IReadOnlyList<ExpenseItem> Items => _items.AsReadOnly();

    /// <summary>
    /// 创建报销单（初始为草稿）
    /// </summary>
    /// <param name="applicantId">申请人用户ID</param>
    /// <param name="applicantName">申请人姓名</param>
    public ExpenseClaim(UserId applicantId, string applicantName)
    {
        ApplicantId = applicantId;
        ApplicantName = applicantName ?? string.Empty;
        Status = ExpenseClaimStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 添加一条报销明细；仅草稿状态可调用
    /// </summary>
    /// <param name="type">费用类型</param>
    /// <param name="amount">金额</param>
    /// <param name="description">说明</param>
    /// <param name="invoiceUrl">发票链接（可选）</param>
    /// <exception cref="KnownException">非草稿状态时抛出</exception>
    public void AddItem(ExpenseType type, decimal amount, string description, string? invoiceUrl = null)
    {
        if (Status != ExpenseClaimStatus.Draft)
            throw new KnownException("只有草稿可修改明细", ErrorCodes.ExpenseClaimInvalidStatus);
        _items.Add(new ExpenseItem(type, amount, description ?? "", invoiceUrl));
        TotalAmount = _items.Sum(i => i.Amount);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 移除指定报销明细；仅草稿状态可调用
    /// </summary>
    /// <param name="itemId">明细ID</param>
    /// <exception cref="KnownException">非草稿状态时抛出</exception>
    public void RemoveItem(ExpenseItemId itemId)
    {
        if (Status != ExpenseClaimStatus.Draft)
            throw new KnownException("只有草稿可修改明细", ErrorCodes.ExpenseClaimInvalidStatus);
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            TotalAmount = _items.Sum(i => i.Amount);
            UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        }
    }

    /// <summary>
    /// 提交报销单；仅草稿状态可调用，且至少有一条明细
    /// </summary>
    /// <param name="workflowInstanceId">关联的工作流实例ID（可选）</param>
    /// <exception cref="KnownException">非草稿或无明细时抛出</exception>
    public void Submit(WorkflowInstanceId? workflowInstanceId = null)
    {
        if (Status != ExpenseClaimStatus.Draft)
            throw new KnownException("只有草稿可提交", ErrorCodes.ExpenseClaimInvalidStatus);
        if (_items.Count == 0)
            throw new KnownException("请至少添加一条报销明细", ErrorCodes.ExpenseClaimInvalidStatus);
        Status = ExpenseClaimStatus.Submitted;
        WorkflowInstanceId = workflowInstanceId;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 审批通过；仅已提交状态可调用
    /// </summary>
    /// <exception cref="KnownException">非已提交状态时抛出</exception>
    public void Approve()
    {
        if (Status != ExpenseClaimStatus.Submitted)
            throw new KnownException("当前状态不允许通过", ErrorCodes.ExpenseClaimInvalidStatus);
        Status = ExpenseClaimStatus.Approved;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 审批驳回；仅已提交状态可调用
    /// </summary>
    /// <exception cref="KnownException">非已提交状态时抛出</exception>
    public void Reject()
    {
        if (Status != ExpenseClaimStatus.Submitted)
            throw new KnownException("当前状态不允许驳回", ErrorCodes.ExpenseClaimInvalidStatus);
        Status = ExpenseClaimStatus.Rejected;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
