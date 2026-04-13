using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 订单合同文件项（上传的合同文件）
/// </summary>
public record OrderContractFileItem(string Path, string FileName, long Size, string Format, string UpdatedAt);

/// <summary>
/// 订单明细行 DTO
/// </summary>
public record OrderItemDto(
    OrderItemId Id,
    ProductId ProductId,
    ProductCategoryId ProductCategoryId,
    ProductTypeId ProductTypeId,
    string ImagePath,
    string InstallNotes,
    string TrainingDuration,
    int PackingStatus,
    int ReviewStatus,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);

/// <summary>
/// 订单按产品分类的合同优惠（order_band）
/// </summary>
public record OrderCategoryDto(
    OrderCategoryId Id,
    ProductCategoryId ProductCategoryId,
    string CategoryName,
    decimal DiscountPoints,
    string Remark);

/// <summary>
/// 订单列表行 DTO
/// </summary>
public record OrderQueryDto(
    OrderId Id,
    CustomerId CustomerId,
    string CustomerName,
    ProjectId ProjectId,
    string ProjectName,
    string OrderNumber,
    OrderType Type,
    OrderStatus Status,
    decimal Amount,
    string Remark,
    UserId OwnerId,
    string OwnerName,
    DeptId DeptId,
    string DeptName,
    string ProjectContactName,
    string ProjectContactPhone,
    string Warranty,
    string ContractSigningCompany,
    string ContractTrustee,
    bool NeedInvoice,
    OrderInvoiceTypeOptionId InvoiceTypeId,
    decimal InstallationFee,
    decimal EstimatedFreight,
    SelectedContractFileId SelectedContractFileId,
    bool IsShipped,
    PaymentStatus PaymentStatus,
    bool ContractNotCompanyTemplate,
    decimal contractAmount,
    OrderLogisticsCompanyId OrderLogisticsCompanyId,
    OrderLogisticsMethodId OrderLogisticsMethodId,
    LogisticsPaymentMethodId LogisticsPaymentMethodId,
    string WaybillNumber,
    decimal ShippingFee,
    bool ShippingFeeIsPay,
    decimal Surcharge,
    bool IsNoLogo,
    string AfterSalesServiceId,
    bool IsAssess,
    string Comments,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool IsRed,
    bool IsFree,
    bool IsRepay,
    DateTimeOffset RepayDate,
    DateTimeOffset FRepayDate,
    DateTimeOffset DelayDate,
    string DelayReason,
    string Feedback,
    string Scontent,
    WarehouseStatus WarehouseStatus,
    UserId WarehousePickerId,
    UserId WarehouseTechId,
    UserId WarehouseReviewerId,
    WorkflowInstanceId WorkflowInstanceId,
    DateTimeOffset CreatedAt);

/// <summary>
/// 订单详情 DTO（含明细）
/// </summary>
public record OrderDetailDto(
    OrderId Id,
    CustomerId CustomerId,
    string CustomerName,
    ProjectId ProjectId,
    string OrderNumber,
    OrderType Type,
    OrderStatus Status,
    decimal Amount,
    string Remark,
    UserId OwnerId,
    string OwnerName,
    DeptId DeptId,
    string DeptName,
    string ProjectContactName,
    string ProjectContactPhone,
    string Warranty,
    string ContractSigningCompany,
    string ContractTrustee,
    bool NeedInvoice,
    OrderInvoiceTypeOptionId InvoiceTypeId,
    decimal InstallationFee,
    decimal EstimatedFreight,
    SelectedContractFileId SelectedContractFileId,
    bool IsShipped,
    PaymentStatus PaymentStatus,
    bool ContractNotCompanyTemplate,
    decimal contractAmount,
    string ReceiverName,
    string ReceiverPhone,
    string ReceiverAddress,
    DateTimeOffset PayDate,
    DateTimeOffset DeliveryDate,
    OrderLogisticsCompanyId OrderLogisticsCompanyId,
    OrderLogisticsMethodId OrderLogisticsMethodId,
    LogisticsPaymentMethodId LogisticsPaymentMethodId,
    string WaybillNumber,
    decimal ShippingFee,
    bool ShippingFeeIsPay,
    decimal Surcharge,
    bool IsNoLogo,
    string AfterSalesServiceId,
    bool IsAssess,
    string Comments,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool IsRed,
    bool IsFree,
    bool IsRepay,
    DateTimeOffset RepayDate,
    DateTimeOffset FRepayDate,
    DateTimeOffset DelayDate,
    string DelayReason,
    string Feedback,
    string Scontent,
    WarehouseStatus WarehouseStatus,
    UserId WarehousePickerId,
    UserId WarehouseTechId,
    UserId WarehouseReviewerId,
    UserId CreatorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    WorkflowInstanceId WorkflowInstanceId,
    IReadOnlyList<OrderContractFileItem> ContractFiles,
    IReadOnlyList<OrderContractFileItem> StockFiles,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderCategoryDto> OrderCategories);

public class OrderQueryInput : PageRequest
{
    public string? OrderNumber { get; set; }
    public CustomerId? CustomerId { get; set; }
    public OrderType? Type { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTimeOffset? CreatedFrom { get; set; }
    public DateTimeOffset? CreatedTo { get; set; }
}

public class OrderQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<OrderDetailDto?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        var o = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (o == null) return null;
        var contractFiles = JsonSerializer.Deserialize<List<OrderContractFileItem>>(o.ContractFilesJson ?? "[]",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        var stockFiles = JsonSerializer.Deserialize<List<OrderContractFileItem>>(o.StockFilesJson ?? "[]",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        var categoryDtos = await (
                from b in dbContext.OrderCategories.AsNoTracking()
                where b.OrderId == id
                select new OrderCategoryDto(b.Id, b.ProductCategoryId, b.CategoryName, b.DiscountPoints, b.Remark))
            .ToListAsync(cancellationToken);
        return new OrderDetailDto(
            o.Id,
            o.CustomerId,
            o.CustomerName,
            o.ProjectId,
            o.OrderNumber,
            o.Type,
            o.Status,
            o.Amount,
            o.Remark,
            o.OwnerId,
            o.OwnerName,
            o.DeptId,
            o.DeptName,
            o.ProjectContactName,
            o.ProjectContactPhone,
            o.Warranty,
            o.ContractSigningCompany,
            o.ContractTrustee,
            o.NeedInvoice,
            o.InvoiceTypeId,
            o.InstallationFee,
            o.EstimatedFreight,
            o.SelectedContractFileId,
            o.IsShipped,
            o.PaymentStatus,
            o.ContractNotCompanyTemplate,
            o.ContractAmount,
            o.ReceiverName,
            o.ReceiverPhone,
            o.ReceiverAddress,
            o.PayDate,
            o.DeliveryDate,
            o.OrderLogisticsCompanyId,
            o.OrderLogisticsMethodId,
            o.LogisticsPaymentMethodId,
            o.WaybillNumber,
            o.ShippingFee,
            o.ShippingFeeIsPay,
            o.Surcharge,
            o.IsNoLogo,
            o.AfterSalesServiceId,
            o.IsAssess,
            o.Comments,
            o.StartDate,
            o.EndDate,
            o.IsRed,
            o.IsFree,
            o.IsRepay,
            o.RepayDate,
            o.FRepayDate,
            o.DelayDate,
            o.DelayReason,
            o.Feedback,
            o.Scontent,
            o.WarehouseStatus,
            o.WarehousePickerId,
            o.WarehouseTechId,
            o.WarehouseReviewerId,
            o.CreatorId,
            o.CreatedAt,
            o.UpdatedAt,
            o.WorkflowInstanceId,
            contractFiles,
            stockFiles,
            o.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductCategoryId,
                i.ProductTypeId,
                i.ImagePath,
                i.InstallNotes,
                i.TrainingDuration,
                i.PackingStatus,
                i.ReviewStatus,
                i.ProductName,
                i.Model,
                i.Number,
                i.Qty,
                i.Unit,
                i.Price,
                i.Amount,
                i.Remark)).ToList(),
            categoryDtos);
    }

    public async Task<PagedData<OrderQueryDto>> GetPagedAsync(OrderQueryInput input, CancellationToken cancellationToken = default)
    {
        var orderQuery = dbContext.Orders.AsNoTracking().Where(o => !o.IsDeleted);
        if (!string.IsNullOrWhiteSpace(input.OrderNumber))
            orderQuery = orderQuery.Where(o => o.OrderNumber.Contains(input.OrderNumber));
        if (input.CustomerId != null)
            orderQuery = orderQuery.Where(o => o.CustomerId == input.CustomerId);
        if (input.Type.HasValue)
            orderQuery = orderQuery.Where(o => o.Type == input.Type.Value);
        if (input.Status.HasValue)
            orderQuery = orderQuery.Where(o => o.Status == input.Status.Value);
        if (input.CreatedFrom.HasValue)
            orderQuery = orderQuery.Where(o => o.CreatedAt >= input.CreatedFrom.Value);
        if (input.CreatedTo.HasValue)
            orderQuery = orderQuery.Where(o => o.CreatedAt <= input.CreatedTo.Value);

        var query = from o in orderQuery
            join p in dbContext.Projects.AsNoTracking() on o.ProjectId equals p.Id into projectJoin
            from p in projectJoin.DefaultIfEmpty()
            select new { Order = o, ProjectName = p != null ? p.Name : string.Empty };

        return await query
            .OrderByDescending(x => x.Order.CreatedAt)
            .Select(x => new OrderQueryDto(
                x.Order.Id,
                x.Order.CustomerId,
                x.Order.CustomerName,
                x.Order.ProjectId,
                x.ProjectName,
                x.Order.OrderNumber,
                x.Order.Type,
                x.Order.Status,
                x.Order.Amount,
                x.Order.Remark,
                x.Order.OwnerId,
                x.Order.OwnerName,
                x.Order.DeptId,
                x.Order.DeptName,
                x.Order.ProjectContactName,
                x.Order.ProjectContactPhone,
                x.Order.Warranty,
                x.Order.ContractSigningCompany,
                x.Order.ContractTrustee,
                x.Order.NeedInvoice,
                x.Order.InvoiceTypeId,
                x.Order.InstallationFee,
                x.Order.EstimatedFreight,
                x.Order.SelectedContractFileId,
                x.Order.IsShipped,
                x.Order.PaymentStatus,
                x.Order.ContractNotCompanyTemplate,
                x.Order.ContractAmount,
                x.Order.OrderLogisticsCompanyId,
                x.Order.OrderLogisticsMethodId,
                x.Order.LogisticsPaymentMethodId,
                x.Order.WaybillNumber,
                x.Order.ShippingFee,
                x.Order.ShippingFeeIsPay,
                x.Order.Surcharge,
                x.Order.IsNoLogo,
                x.Order.AfterSalesServiceId,
                x.Order.IsAssess,
                x.Order.Comments,
                x.Order.StartDate,
                x.Order.EndDate,
                x.Order.IsRed,
                x.Order.IsFree,
                x.Order.IsRepay,
                x.Order.RepayDate,
                x.Order.FRepayDate,
                x.Order.DelayDate,
                x.Order.DelayReason,
                x.Order.Feedback,
                x.Order.Scontent,
                x.Order.WarehouseStatus,
                x.Order.WarehousePickerId,
                x.Order.WarehouseTechId,
                x.Order.WarehouseReviewerId,
                x.Order.WorkflowInstanceId,
                x.Order.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
