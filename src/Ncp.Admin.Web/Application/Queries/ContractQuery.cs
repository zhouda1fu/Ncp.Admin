using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 合同发票 DTO（子实体）
/// </summary>
public record ContractInvoiceDto(
    ContractInvoiceId Id,
    InvoiceType Type,
    string InvoiceNumber,
    decimal TaxRate,
    decimal AmountExclTax,
    string Source,
    bool Status,
    string Title,
    decimal TaxAmount,
    decimal InvoicedAmount,
    string Handler,
    DateTimeOffset BillingDate,
    string Remarks,
    string AttachmentStorageKey);

public record ContractQueryDto(
    ContractId Id,
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    ContractStatus Status,
    string? FileStorageKey,
    UserId CreatorId,
    DateTimeOffset CreatedAt,
    OrderId? OrderId,
    CustomerId? CustomerId,
    string? CustomerName,
    int ContractType,
    string? ContractTypeName,
    int IncomeExpenseType,
    string? IncomeExpenseTypeName,
    DateTimeOffset? SignDate,
    string? Note,
    string? Description,
    UserId? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    bool HasAttachment,
    Guid? DepartmentId,
    string? BusinessManager,
    string? ResponsibleProject,
    string? InputCustomer,
    bool NextPaymentReminder,
    bool ContractExpiryReminder,
    int? SingleDoubleSeal,
    string? InvoicingInformation,
    int? PaymentStatus,
    string? WarrantyPeriod,
    bool IsInstallmentPayment,
    decimal? AccumulatedAmount,
    IReadOnlyList<ContractInvoiceDto>? Invoices = null);

public class ContractQueryInput : PageRequest
{
    public string? Code { get; set; }
    public string? Title { get; set; }
    public ContractStatus? Status { get; set; }
    /// <summary>订单 ID（Guid 字符串，用于列表筛选）</summary>
    public string? OrderId { get; set; }
    /// <summary>客户 ID（Guid 字符串，用于列表筛选）</summary>
    public string? CustomerId { get; set; }
    public int? ContractType { get; set; }
    public int? IncomeExpenseType { get; set; }
    public DateTimeOffset? SignDateFrom { get; set; }
    public DateTimeOffset? SignDateTo { get; set; }
}

public class ContractQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<ContractQueryDto?> GetByIdAsync(ContractId id, CancellationToken cancellationToken = default)
    {
        var contract = await dbContext.Contracts
            .AsNoTracking()
            .Include(c => c.Invoices)
            .Where(c => c.Id == id && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
        if (contract == null) return null;
        var customerName = await dbContext.Customers
            .Where(cust => cust.Id == contract.CustomerId)
            .Select(cust => cust.FullName)
            .FirstOrDefaultAsync(cancellationToken);
        var invoices = contract.Invoices
            .Select(i => new ContractInvoiceDto(
                i.Id, i.Type, i.InvoiceNumber, i.TaxRate, i.AmountExclTax, i.Source, i.Status,
                i.Title, i.TaxAmount, i.InvoicedAmount, i.Handler, i.BillingDate, i.Remarks, i.AttachmentStorageKey))
            .ToList();
        return new ContractQueryDto(
            contract.Id, contract.Code, contract.Title, contract.PartyA, contract.PartyB, contract.Amount,
            contract.StartDate, contract.EndDate, contract.Status, contract.FileStorageKey,
            contract.CreatorId, contract.CreatedAt, contract.OrderId, contract.CustomerId, customerName,
            contract.ContractType, contract.ContractTypeName, contract.IncomeExpenseType, contract.IncomeExpenseTypeName,
            contract.SignDate, contract.Note, contract.Description, contract.ApprovedBy, contract.ApprovedAt,
            !string.IsNullOrEmpty(contract.FileStorageKey),
            contract.DepartmentId, contract.BusinessManager, contract.ResponsibleProject, contract.InputCustomer,
            contract.NextPaymentReminder, contract.ContractExpiryReminder, contract.SingleDoubleSeal,
            contract.InvoicingInformation, contract.PaymentStatus, contract.WarrantyPeriod,
            contract.IsInstallmentPayment, contract.AccumulatedAmount,
            invoices);
    }

    public async Task<PagedData<ContractQueryDto>> GetPagedAsync(ContractQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Contracts.AsNoTracking().Where(c => !c.IsDeleted);
        if (!string.IsNullOrWhiteSpace(input.Code))
            query = query.Where(c => c.Code.Contains(input.Code));
        if (!string.IsNullOrWhiteSpace(input.Title))
            query = query.Where(c => c.Title.Contains(input.Title));
        if (input.Status.HasValue)
            query = query.Where(c => c.Status == input.Status.Value);
        if (!string.IsNullOrWhiteSpace(input.OrderId) && Guid.TryParse(input.OrderId, out var orderIdGuid))
            query = query.Where(c => c.OrderId == new OrderId(orderIdGuid));
        if (!string.IsNullOrWhiteSpace(input.CustomerId) && Guid.TryParse(input.CustomerId, out var customerIdGuid))
            query = query.Where(c => c.CustomerId == new CustomerId(customerIdGuid));
        if (input.ContractType.HasValue)
            query = query.Where(c => c.ContractType == input.ContractType.Value);
        if (input.IncomeExpenseType.HasValue)
            query = query.Where(c => c.IncomeExpenseType == input.IncomeExpenseType.Value);
        if (input.SignDateFrom.HasValue)
            query = query.Where(c => c.SignDate >= input.SignDateFrom.Value);
        if (input.SignDateTo.HasValue)
            query = query.Where(c => c.SignDate <= input.SignDateTo.Value);
        return await query
            .OrderByDescending(c => c.SignDate)
            .Select(c => new ContractQueryDto(
                c.Id, c.Code, c.Title, c.PartyA, c.PartyB, c.Amount, c.StartDate, c.EndDate, c.Status, c.FileStorageKey,
                c.CreatorId, c.CreatedAt, c.OrderId, c.CustomerId,
                dbContext.Customers.Where(cust => cust.Id == c.CustomerId).Select(cust => cust.FullName).FirstOrDefault(),
                c.ContractType, c.ContractTypeName, c.IncomeExpenseType, c.IncomeExpenseTypeName, c.SignDate, c.Note, c.Description, c.ApprovedBy, c.ApprovedAt,
                c.FileStorageKey != null && c.FileStorageKey != "",
                c.DepartmentId, c.BusinessManager, c.ResponsibleProject, c.InputCustomer,
                c.NextPaymentReminder, c.ContractExpiryReminder, c.SingleDoubleSeal, c.InvoicingInformation, c.PaymentStatus, c.WarrantyPeriod,
                c.IsInstallmentPayment, c.AccumulatedAmount,
                null))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
