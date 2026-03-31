using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;

/// <summary>
/// 订单发票类型选项 ID（强类型）
/// </summary>
public partial record OrderInvoiceTypeOptionId : IInt64StronglyTypedId;

/// <summary>
/// 订单发票类型选项
/// </summary>
public class OrderInvoiceTypeOption : Entity<OrderInvoiceTypeOptionId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected OrderInvoiceTypeOption() { }

    /// <summary>名称</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>类型值</summary>
    public int TypeValue { get; private set; }

    /// <summary>排序</summary>
    public int SortOrder { get; private set; }

    public OrderInvoiceTypeOption(string name, int typeValue, int sortOrder)
    {
        Name = name;
        TypeValue = typeValue;
        SortOrder = sortOrder;
    }

    public void Update(string name, int typeValue, int sortOrder)
    {
        Name = name;
        TypeValue = typeValue;
        SortOrder = sortOrder;
    }
}
