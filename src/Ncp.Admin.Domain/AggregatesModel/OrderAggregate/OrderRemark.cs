using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单备注 ID（强类型）
/// </summary>
public partial record OrderRemarkId : IGuidStronglyTypedId;

/// <summary>
/// 订单备注行（属 Order 聚合）：记录添加时间与说明内容
/// </summary>
public class OrderRemark : Entity<OrderRemarkId>
{
    /// <summary>EF/序列化用</summary>
    protected OrderRemark() { }

    /// <summary>所属订单 ID</summary>
    public OrderId OrderId { get; private set; } = default!;

    /// <summary>添加时间</summary>
    public DateTimeOffset AddedAt { get; private set; }

    /// <summary>用户ID</summary>
    public UserId UserId { get; private set; } = default!;

    /// <summary>类型ID 0:分期备注 1：优惠点数说明</summary>
    public int TypeId { get; private set; }

    /// <summary>说明内容</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// 修改备注内容（仅更新 Content，不调整添加时间与类型）
    /// </summary>
    public void ChangeContent(string content)
    {
        Content = content ?? string.Empty;
    }

    /// <summary>
    /// 创建备注行（由聚合根调用）；OrderId 由 EF 在保存时通过关联设置
    /// </summary>
    internal static OrderRemark Create(
        string content,
        UserId userId,
        int typeId,
        DateTimeOffset? addedAt = null)
    {
        return new OrderRemark
        {
            Content = content ?? string.Empty,
            UserId = userId,
            TypeId = typeId,
            AddedAt = addedAt ?? DateTimeOffset.UtcNow,
        };
    }
}
