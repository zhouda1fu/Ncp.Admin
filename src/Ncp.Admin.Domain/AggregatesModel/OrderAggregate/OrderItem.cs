using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单明细 ID（强类型）
/// </summary>
public partial record OrderItemId : IGuidStronglyTypedId;

/// <summary>
/// 订单明细行（属 Order 聚合），关联产品
/// </summary>
public class OrderItem : Entity<OrderItemId>
{
    /// <summary>EF/序列化用</summary>
    protected OrderItem() { }

    /// <summary>所属订单 ID</summary>
    public OrderId OrderId { get; private set; } = default!;

    /// <summary>产品 ID（必填）</summary>
    public ProductId ProductId { get; private set; } = default!;

    /// <summary>产品名称（冗余）</summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>型号</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>货号/编号</summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>数量</summary>
    public int Qty { get; private set; }

    /// <summary>单位</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>单价</summary>
    public decimal Price { get; private set; }

    /// <summary>金额</summary>
    public decimal Amount { get; private set; }

    /// <summary>备注</summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>
    /// 创建明细行（由聚合根调用）；OrderId 由 EF 在保存时通过关联设置
    /// </summary>
    internal static OrderItem Create(OrderItemData data)
    {
        return new OrderItem
        {
            ProductId = data.ProductId,
            ProductName = data.ProductName ?? string.Empty,
            Model = data.Model ?? string.Empty,
            Number = data.Number ?? string.Empty,
            Qty = data.Qty,
            Unit = data.Unit ?? string.Empty,
            Price = data.Price,
            Amount = data.Amount,
            Remark = data.Remark ?? string.Empty,
        };
    }
}
