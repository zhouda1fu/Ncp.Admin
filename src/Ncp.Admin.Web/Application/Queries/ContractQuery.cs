using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

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
    int? SingleDoubleProfit,
    string? InvoicingInformation,
    int? PaymentStatus,
    string? WarrantyPeriod,
    bool IsInstallmentPayment,
    decimal? AccumulatedAmount);

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
        var query = dbContext.Contracts
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted);
        return await query
            .Select(c => new ContractQueryDto(
                c.Id, c.Code, c.Title, c.PartyA, c.PartyB, c.Amount, c.StartDate, c.EndDate, c.Status, c.FileStorageKey,
                c.CreatorId, c.CreatedAt, c.OrderId, c.CustomerId,
                dbContext.Customers.Where(cust => cust.Id == c.CustomerId).Select(cust => cust.FullName).FirstOrDefault(),
                c.ContractType, c.ContractTypeName, c.IncomeExpenseType, c.IncomeExpenseTypeName, c.SignDate, c.Note, c.Description, c.ApprovedBy, c.ApprovedAt,
                c.FileStorageKey != null && c.FileStorageKey != "",
                c.DepartmentId, c.BusinessManager, c.ResponsibleProject, c.InputCustomer,
                c.NextPaymentReminder, c.ContractExpiryReminder, c.SingleDoubleProfit, c.InvoicingInformation, c.PaymentStatus, c.WarrantyPeriod,
                c.IsInstallmentPayment, c.AccumulatedAmount))
            .FirstOrDefaultAsync(cancellationToken);
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
                c.NextPaymentReminder, c.ContractExpiryReminder, c.SingleDoubleProfit, c.InvoicingInformation, c.PaymentStatus, c.WarrantyPeriod,
                c.IsInstallmentPayment, c.AccumulatedAmount))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
