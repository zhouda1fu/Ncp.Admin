using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单分类 ID（强类型）
/// </summary>
public partial record OrderCategoryId : IGuidStronglyTypedId;

/// <summary>
/// 订单分类合同优惠
/// </summary>
public class OrderCategory : Entity<OrderCategoryId>
{
    /// <summary>EF/序列化用</summary>
    protected OrderCategory() { }

    /// <summary>订单ID</summary>
    public OrderId OrderId { get; private set; } = default!;

    /// <summary>产品分类ID</summary>
    public ProductCategoryId ProductCategoryId { get; private set; } = default!;

    /// <summary>产品分类名称（冗余）</summary>
    public string CategoryName { get; private set; } = string.Empty;

    /// <summary>优惠点数</summary>
    public decimal DiscountPoints { get; private set; }

    /// <summary>备注</summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>新建分类优惠行（业务；EF 使用无参 <see cref="OrderCategory" />）</summary>
    public OrderCategory(
        OrderId orderId,
        ProductCategoryId productCategoryId,
        string categoryName,
        decimal discountPoints,
        string remark)
    {
        OrderId = orderId;
        ProductCategoryId = productCategoryId;
        CategoryName = categoryName ?? string.Empty;
        DiscountPoints = discountPoints;
        Remark = remark ?? string.Empty;
    }

    /// <summary>更新</summary>
    public void Update(ProductCategoryId productCategoryId, string categoryName, decimal discountPoints, string remark)
    {
        ProductCategoryId = productCategoryId;
        CategoryName = categoryName ?? string.Empty;
        DiscountPoints = discountPoints;
        Remark = remark ?? string.Empty;
    }
}
