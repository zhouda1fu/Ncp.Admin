using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ContractAggregate;

/// <summary>
/// 合同发票 ID（强类型）
/// </summary>
public partial record ContractInvoiceId : IGuidStronglyTypedId;

/// <summary>
/// 发票类型（枚举）
/// </summary>
public enum InvoiceType
{
    /// <summary>增值税专用发票</summary>
    VatSpecial = 0,
    /// <summary>增值税普通发票</summary>
    VatGeneral = 1,
    /// <summary>收据</summary>
    Receipt = 2,
    /// <summary>航空运输电子客票行程单</summary>
    AirEticketItinerary = 3,
    /// <summary>铁路电子客票</summary>
    RailwayEticket = 4,
}

/// <summary>
/// 合同发票子实体（合同一对多）
/// </summary>
public class ContractInvoice : Entity<ContractInvoiceId>
{
    protected ContractInvoice() { }

    /// <summary>所属合同 ID</summary>
    public ContractId ContractId { get; private set; } = default!;
    /// <summary>发票类型</summary>
    public InvoiceType Type { get; private set; }
    /// <summary>发票编号</summary>
    public string InvoiceNumber { get; private set; } = string.Empty;
    /// <summary>发票税率（如 0.0 表示 0.0%）</summary>
    public decimal TaxRate { get; private set; }
    /// <summary>不含税金额</summary>
    public decimal AmountExclTax { get; private set; }
    /// <summary>来源</summary>
    public string Source { get; private set; } = string.Empty;
    /// <summary>状态：false=待确认，true=已确认</summary>
    public bool Status { get; private set; }
    /// <summary>标题</summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>税额</summary>
    public decimal TaxAmount { get; private set; }
    /// <summary>开票金额</summary>
    public decimal InvoicedAmount { get; private set; }
    /// <summary>经手人（可选）</summary>
    public string Handler { get; private set; } = string.Empty;
    /// <summary>开票日期</summary>
    public DateTimeOffset BillingDate { get; private set; }
    /// <summary>备注说明</summary>
    public string Remarks { get; private set; } = string.Empty;
    /// <summary>附件存储 Key（单文件或 JSON 多文件，5MB 以内）</summary>
    public string AttachmentStorageKey { get; private set; } = string.Empty;

    internal ContractInvoice(
        ContractId contractId,
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
        ContractId = contractId;
        Type = type;
        InvoiceNumber = invoiceNumber ?? string.Empty;
        TaxRate = taxRate;
        AmountExclTax = amountExclTax;
        Source = source ?? string.Empty;
        Status = status;
        Title = title ?? string.Empty;
        TaxAmount = taxAmount;
        InvoicedAmount = invoicedAmount;
        Handler = handler ?? string.Empty;
        BillingDate = billingDate;
        Remarks = remarks ?? string.Empty;
        AttachmentStorageKey = attachmentStorageKey ?? string.Empty;
    }

    internal void Update(
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
        Type = type;
        InvoiceNumber = invoiceNumber ?? string.Empty;
        TaxRate = taxRate;
        AmountExclTax = amountExclTax;
        Source = source ?? string.Empty;
        Status = status;
        Title = title ?? string.Empty;
        TaxAmount = taxAmount;
        InvoicedAmount = invoicedAmount;
        Handler = handler ?? string.Empty;
        BillingDate = billingDate;
        Remarks = remarks ?? string.Empty;
        AttachmentStorageKey = attachmentStorageKey ?? string.Empty;
    }
}
