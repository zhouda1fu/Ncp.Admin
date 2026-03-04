using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 订单明细行 DTO
/// </summary>
public record OrderItemDto(
    OrderItemId Id,
    ProductId ProductId,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);

/// <summary>
/// 订单列表行 DTO
/// </summary>
public record OrderQueryDto(
    OrderId Id,
    CustomerId CustomerId,
    string CustomerName,
    ProjectId ProjectId,
    ContractId ContractId,
    string OrderNumber,
    OrderType Type,
    OrderStatus Status,
    decimal Amount,
    string Remark,
    UserId OwnerId,
    string OwnerName,
    DateTimeOffset CreatedAt);

/// <summary>
/// 订单详情 DTO（含明细）
/// </summary>
public record OrderDetailDto(
    OrderId Id,
    CustomerId CustomerId,
    string CustomerName,
    ProjectId ProjectId,
    ContractId ContractId,
    string OrderNumber,
    OrderType Type,
    OrderStatus Status,
    decimal Amount,
    string Remark,
    UserId OwnerId,
    string OwnerName,
    string ReceiverName,
    string ReceiverPhone,
    string ReceiverAddress,
    DateTimeOffset PayDate,
    DateTimeOffset DeliveryDate,
    UserId CreatorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<OrderItemDto> Items);

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
        return new OrderDetailDto(
            o.Id,
            o.CustomerId,
            o.CustomerName,
            o.ProjectId,
            o.ContractId,
            o.OrderNumber,
            o.Type,
            o.Status,
            o.Amount,
            o.Remark,
            o.OwnerId,
            o.OwnerName,
            o.ReceiverName,
            o.ReceiverPhone,
            o.ReceiverAddress,
            o.PayDate,
            o.DeliveryDate,
            o.CreatorId,
            o.CreatedAt,
            o.UpdatedAt,
            o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.Model, i.Number, i.Qty, i.Unit, i.Price, i.Amount, i.Remark)).ToList());
    }

    public async Task<PagedData<OrderQueryDto>> GetPagedAsync(OrderQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking().Where(o => !o.IsDeleted);
        if (!string.IsNullOrWhiteSpace(input.OrderNumber))
            query = query.Where(o => o.OrderNumber.Contains(input.OrderNumber));
        if (input.CustomerId != null)
            query = query.Where(o => o.CustomerId == input.CustomerId);
        if (input.Type.HasValue)
            query = query.Where(o => o.Type == input.Type.Value);
        if (input.Status.HasValue)
            query = query.Where(o => o.Status == input.Status.Value);
        if (input.CreatedFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= input.CreatedFrom.Value);
        if (input.CreatedTo.HasValue)
            query = query.Where(o => o.CreatedAt <= input.CreatedTo.Value);
        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderQueryDto(
                o.Id,
                o.CustomerId,
                o.CustomerName,
                o.ProjectId,
                o.ContractId,
                o.OrderNumber,
                o.Type,
                o.Status,
                o.Amount,
                o.Remark,
                o.OwnerId,
                o.OwnerName,
                o.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
