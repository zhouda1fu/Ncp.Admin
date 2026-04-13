using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;

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

    /// <summary>
    /// 产品分类
    /// </summary>
    public ProductCategoryId ProductCategoryId { get; private set; } = default!;

    /// <summary>
    /// 产品类型
    /// </summary>

    public ProductTypeId ProductTypeId { get; private set; } = default!;
    /// <summary>
    /// 图片地址
    /// </summary>
    public string ImagePath { get; private set; } = string.Empty;

    /// <summary>
    /// 安装完成情况
    /// </summary>
    public string InstallNotes { get; private set; } = string.Empty;

    /// <summary>
    /// 培训时长
    /// </summary>
    public string TrainingDuration { get; private set; } = string.Empty;

    /// <summary>
    /// 配货状态 0：未配货 1：已配货
    /// </summary>
    public int PackingStatus { get; private set; }

    /// <summary>
    /// 复核状态 0未复核 1：已复核
    /// </summary>
    public int ReviewStatus { get; private set; }



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

    /// <summary>新建明细行（由 <see cref="Order"/> 调用）；OrderId 由 EF 在保存时通过关联设置</summary>
    internal OrderItem(OrderItemData data)
    {
        ProductId = data.ProductId;
        ProductCategoryId = data.ProductCategoryId;
        ProductTypeId = data.ProductTypeId;
        ImagePath = data.ImagePath ?? string.Empty;
        InstallNotes = data.InstallNotes ?? string.Empty;
        TrainingDuration = data.TrainingDuration ?? string.Empty;
        PackingStatus = data.PackingStatus;
        ReviewStatus = data.ReviewStatus;
        ProductName = data.ProductName ?? string.Empty;
        Model = data.Model ?? string.Empty;
        Number = data.Number ?? string.Empty;
        Qty = data.Qty;
        Unit = data.Unit ?? string.Empty;
        Price = data.Price;
        Amount = data.Amount;
        Remark = data.Remark ?? string.Empty;
    }
}
