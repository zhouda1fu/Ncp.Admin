using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ContractAggregate;

/// <summary>
/// 合同ID（强类型ID）
/// </summary>
public partial record ContractId : IGuidStronglyTypedId;

/// <summary>
/// 合同状态
/// </summary>
public enum ContractStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,
    /// <summary>
    /// 审批中
    /// </summary>
    PendingApproval = 1,
    /// <summary>
    /// 已生效
    /// </summary>
    Approved = 2,
    /// <summary>
    /// 已归档
    /// </summary>
    Archived = 3,
}

/// <summary>
/// 合同聚合根：合同创建、审批、到期提醒、归档
/// </summary>
public class Contract : Entity<ContractId>, IAggregateRoot
{
    protected Contract() { }

    /// <summary>
    /// 合同编号
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 合同标题
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 甲方
    /// </summary>
    public string PartyA { get; private set; } = string.Empty;
    /// <summary>
    /// 乙方
    /// </summary>
    public string PartyB { get; private set; } = string.Empty;
    /// <summary>
    /// 合同金额
    /// </summary>
    public decimal Amount { get; private set; }
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTimeOffset StartDate { get; private set; }
    /// <summary>
    /// 结束日期（到期日）
    /// </summary>
    public DateTimeOffset EndDate { get; private set; }
    /// <summary>
    /// 状态
    /// </summary>
    public ContractStatus Status { get; private set; }
    /// <summary>
    /// 附件存储 Key（可选）
    /// </summary>
    public string? FileStorageKey { get; private set; }
    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建合同（草稿）
    /// </summary>
    public Contract(string code, string title, string partyA, string partyB, decimal amount, DateTimeOffset startDate, DateTimeOffset endDate, UserId creatorId, string? fileStorageKey = null)
    {
        Code = code ;
        Title = title ;
        PartyA = partyA ;
        PartyB = partyB ;
        Amount = amount;
        StartDate = startDate;
        EndDate = endDate;
        CreatorId = creatorId;
        FileStorageKey = fileStorageKey;
        Status = ContractStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新合同信息（仅草稿可改）
    /// </summary>
    public void Update(string code, string title, string partyA, string partyB, decimal amount, DateTimeOffset startDate, DateTimeOffset endDate, string? fileStorageKey = null)
    {
        if (Status != ContractStatus.Draft)
            throw new KnownException("仅草稿状态可修改", ErrorCodes.ContractNotDraft);
        Code = code ;
        Title = title ;
        PartyA = partyA ;
        PartyB = partyB ;
        Amount = amount;
        StartDate = startDate;
        EndDate = endDate;
        FileStorageKey = fileStorageKey;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 提交审批
    /// </summary>
    public void SubmitForApproval()
    {
        if (Status != ContractStatus.Draft)
            throw new KnownException("仅草稿可提交审批", ErrorCodes.ContractNotDraft);
        Status = ContractStatus.PendingApproval;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 审批通过
    /// </summary>
    public void Approve()
    {
        if (Status != ContractStatus.PendingApproval)
            throw new KnownException("仅审批中可通过", ErrorCodes.ContractNotPendingApproval);
        Status = ContractStatus.Approved;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 归档
    /// </summary>
    public void Archive()
    {
        if (Status != ContractStatus.Approved)
            throw new KnownException("仅已生效合同可归档", ErrorCodes.ContractNotApproved);
        Status = ContractStatus.Archived;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
