using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents;

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
    /// 附件存储 Key
    /// </summary>
    public string FileStorageKey { get; private set; } = string.Empty;
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
    /// 关联订单 ID（空为未关联）
    /// </summary>
    public OrderId OrderId { get; private set; } = default!;
    /// <summary>
    /// 关联客户 ID（空为未关联）
    /// </summary>
    public CustomerId CustomerId { get; private set; } = default!;
    /// <summary>
    /// 合同类型（类型值，对应 ContractTypeOption.TypeValue）
    /// </summary>
    public int ContractType { get; private set; }
    /// <summary>
    /// 合同类型名称（冗余，前端“合同签订公司”展示用）
    /// </summary>
    public string ContractTypeName { get; private set; } = string.Empty;
    /// <summary>
    /// 收支类型（类型值，对应 IncomeExpenseTypeOption.TypeValue）
    /// </summary>
    public int IncomeExpenseType { get; private set; }
    /// <summary>
    /// 收支类型名称（冗余）
    /// </summary>
    public string IncomeExpenseTypeName { get; private set; } = string.Empty;
    /// <summary>
    /// 签约日期
    /// </summary>
    public DateTimeOffset SignDate { get; private set; }
    /// <summary>
    /// 部门 ID
    /// </summary>
    public DeptId DepartmentId { get; private set; } = default!;
    /// <summary>
    /// 业务经理
    /// </summary>
    public string BusinessManager { get; private set; } = string.Empty;
    /// <summary>
    /// 负责项目
    /// </summary>
    public string ResponsibleProject { get; private set; } = string.Empty;
    /// <summary>
    /// 录入客户（名称或标识）
    /// </summary>
    public string InputCustomer { get; private set; } = string.Empty;
    /// <summary>
    /// 下次收付款报警
    /// </summary>
    public bool NextPaymentReminder { get; private set; }
    /// <summary>
    /// 合同过期报警
    /// </summary>
    public bool ContractExpiryReminder { get; private set; }
    /// <summary>
    /// 单双章（类型值，0=单章 1=双章）
    /// </summary>
    public int SingleDoubleSeal { get; private set; }
    /// <summary>
    /// 开票信息
    /// </summary>
    public string InvoicingInformation { get; private set; } = string.Empty;
    /// <summary>
    /// 到款情况（类型值）
    /// </summary>
    public int PaymentStatus { get; private set; }
    /// <summary>
    /// 质保期
    /// </summary>
    public string WarrantyPeriod { get; private set; } = string.Empty;
    /// <summary>
    /// 是否分期
    /// </summary>
    public bool IsInstallmentPayment { get; private set; }
    /// <summary>
    /// 累计金额
    /// </summary>
    public decimal AccumulatedAmount { get; private set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string Note { get; private set; } = string.Empty;
    /// <summary>
    /// 合同内容/描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>
    /// 审批人（审批通过时记录，未审批为 default）
    /// </summary>
    public UserId ApprovedBy { get; private set; } = default!;
    /// <summary>
    /// 审批时间（未审批为 default）
    /// </summary>
    public DateTimeOffset ApprovedAt { get; private set; }

    /// <summary>
    /// 并发版本
    /// </summary>
    public RowVersion RowVersion { get; private set; } = new RowVersion(0);

    /// <summary>
    /// 软删标记
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 发票列表（一对多子实体）
    /// </summary>
    public virtual ICollection<ContractInvoice> Invoices { get; } = [];

    /// <summary>
    /// 创建合同（草稿）
    /// </summary>
    public Contract(
        string code,
        string title,
        string partyA,
        string partyB,
        decimal amount,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        UserId creatorId,
        string fileStorageKey,
        OrderId orderId,
        CustomerId customerId,
        int contractType,
        string contractTypeName,
        int incomeExpenseType,
        string incomeExpenseTypeName,
        DateTimeOffset signDate,
        string note,
        string description,
        DeptId departmentId,
        string businessManager,
        string responsibleProject,
        string inputCustomer,
        bool nextPaymentReminder,
        bool contractExpiryReminder,
        int singleDoubleSeal,
        string invoicingInformation,
        int paymentStatus,
        string warrantyPeriod,
        bool isInstallmentPayment,
        decimal accumulatedAmount)
    {
        Code = code ?? string.Empty;
        Title = title ?? string.Empty;
        PartyA = partyA ?? string.Empty;
        PartyB = partyB ?? string.Empty;
        Amount = amount;
        StartDate = startDate;
        EndDate = endDate;
        CreatorId = creatorId;
        FileStorageKey = fileStorageKey ?? string.Empty;
        OrderId = orderId;
        CustomerId = customerId;
        ContractType = contractType;
        ContractTypeName = contractTypeName ?? string.Empty;
        IncomeExpenseType = incomeExpenseType;
        IncomeExpenseTypeName = incomeExpenseTypeName ?? string.Empty;
        SignDate = signDate;
        Note = note ?? string.Empty;
        Description = description ?? string.Empty;
        DepartmentId = departmentId;
        BusinessManager = businessManager ?? string.Empty;
        ResponsibleProject = responsibleProject ?? string.Empty;
        InputCustomer = inputCustomer ?? string.Empty;
        NextPaymentReminder = nextPaymentReminder;
        ContractExpiryReminder = contractExpiryReminder;
        SingleDoubleSeal = singleDoubleSeal;
        InvoicingInformation = invoicingInformation ?? string.Empty;
        PaymentStatus = paymentStatus;
        WarrantyPeriod = warrantyPeriod ?? string.Empty;
        IsInstallmentPayment = isInstallmentPayment;
        AccumulatedAmount = accumulatedAmount;
        Status = ContractStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ContractCreatedDomainEvent(this));
    }

    /// <summary>
    /// 更新合同信息（仅草稿可改）
    /// </summary>
    public void Update(
        string code,
        string title,
        string partyA,
        string partyB,
        decimal amount,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string fileStorageKey,
        OrderId orderId,
        CustomerId customerId,
        int contractType,
        string contractTypeName,
        int incomeExpenseType,
        string incomeExpenseTypeName,
        DateTimeOffset signDate,
        string note,
        string description,
        DeptId departmentId,
        string businessManager,
        string responsibleProject,
        string inputCustomer,
        bool nextPaymentReminder,
        bool contractExpiryReminder,
        int singleDoubleSeal,
        string invoicingInformation,
        int paymentStatus,
        string warrantyPeriod,
        bool isInstallmentPayment,
        decimal accumulatedAmount)
    {
        if (Status != ContractStatus.Draft)
            throw new KnownException("仅草稿状态可修改", ErrorCodes.ContractNotDraft);
        Code = code ?? string.Empty;
        Title = title ?? string.Empty;
        PartyA = partyA ?? string.Empty;
        PartyB = partyB ?? string.Empty;
        Amount = amount;
        StartDate = startDate;
        EndDate = endDate;
        FileStorageKey = fileStorageKey ?? string.Empty;
        OrderId = orderId;
        CustomerId = customerId;
        ContractType = contractType;
        ContractTypeName = contractTypeName ?? string.Empty;
        IncomeExpenseType = incomeExpenseType;
        IncomeExpenseTypeName = incomeExpenseTypeName ?? string.Empty;
        SignDate = signDate;
        Note = note ?? string.Empty;
        Description = description ?? string.Empty;
        DepartmentId = departmentId;
        BusinessManager = businessManager ?? string.Empty;
        ResponsibleProject = responsibleProject ?? string.Empty;
        InputCustomer = inputCustomer ?? string.Empty;
        NextPaymentReminder = nextPaymentReminder;
        ContractExpiryReminder = contractExpiryReminder;
        SingleDoubleSeal = singleDoubleSeal;
        InvoicingInformation = invoicingInformation ?? string.Empty;
        PaymentStatus = paymentStatus;
        WarrantyPeriod = warrantyPeriod ?? string.Empty;
        IsInstallmentPayment = isInstallmentPayment;
        AccumulatedAmount = accumulatedAmount;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new ContractUpdatedDomainEvent(this));
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
        AddDomainEvent(new ContractSubmitForApprovalDomainEvent(this));
    }

    /// <summary>
    /// 审批通过
    /// </summary>
    public void Approve(UserId approvedBy)
    {
        if (Status != ContractStatus.PendingApproval)
            throw new KnownException("仅审批中可通过", ErrorCodes.ContractNotPendingApproval);
        Status = ContractStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTimeOffset.UtcNow;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new ContractApprovedDomainEvent(this));
    }

    /// <summary>
    /// 软删除（仅草稿可删）
    /// </summary>
    public void MarkDeleted()
    {
        if (Status != ContractStatus.Draft)
            throw new KnownException("仅草稿状态可删除", ErrorCodes.ContractNotDraft);
        IsDeleted = new Deleted(true);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new ContractDeletedDomainEvent(this));
    }

    /// <summary>
    /// 新增发票（仅草稿可操作）
    /// </summary>
    public ContractInvoice AddInvoice(
        InvoiceType type,
        string invoiceNumber,
        decimal taxRate,
        decimal amountExclTax,
        string source,
        bool status,
        string title,
        decimal taxAmount,
        decimal invoicedAmount,
        string handler,
        DateTimeOffset billingDate,
        string remarks,
        string attachmentStorageKey)
    {
        if (Status != ContractStatus.Draft)
            throw new KnownException("仅草稿状态可新增发票", ErrorCodes.ContractNotDraft);
        var invoice = new ContractInvoice(
            Id, type, invoiceNumber, taxRate, amountExclTax, source, status,
            title, taxAmount, invoicedAmount, handler, billingDate, remarks, attachmentStorageKey);
        Invoices.Add(invoice);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new ContractUpdatedDomainEvent(this));
        return invoice;
    }

    /// <summary>
    /// 更新发票（仅草稿可操作）
    /// </summary>
    public void UpdateInvoice(
        ContractInvoiceId invoiceId,
        InvoiceType type,
        string invoiceNumber,
        decimal taxRate,
        decimal amountExclTax,
        string source,
        bool status,
        string title,
        decimal taxAmount,
        decimal invoicedAmount,
        string handler,
        DateTimeOffset billingDate,
        string remarks,
        string attachmentStorageKey)
    {
        if (Status != ContractStatus.Draft)
            throw new KnownException("仅草稿状态可修改发票", ErrorCodes.ContractNotDraft);
        var invoice = Invoices.FirstOrDefault(x => x.Id == invoiceId)
            ?? throw new KnownException("未找到该发票", ErrorCodes.ContractInvoiceNotFound);
        invoice.Update(type, invoiceNumber, taxRate, amountExclTax, source, status,
            title, taxAmount, invoicedAmount, handler, billingDate, remarks, attachmentStorageKey);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new ContractUpdatedDomainEvent(this));
    }

    /// <summary>
    /// 移除发票（仅草稿可操作）
    /// </summary>
    public void RemoveInvoice(ContractInvoiceId invoiceId)
    {
        if (Status != ContractStatus.Draft)
            throw new KnownException("仅草稿状态可删除发票", ErrorCodes.ContractNotDraft);
        var invoice = Invoices.FirstOrDefault(x => x.Id == invoiceId)
            ?? throw new KnownException("未找到该发票", ErrorCodes.ContractInvoiceNotFound);
        Invoices.Remove(invoice);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new ContractUpdatedDomainEvent(this));
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
        AddDomainEvent(new ContractArchivedDomainEvent(this));
    }
}
