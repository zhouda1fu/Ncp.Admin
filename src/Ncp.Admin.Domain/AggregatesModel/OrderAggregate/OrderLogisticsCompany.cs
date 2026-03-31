using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单物流公司 ID (强类型)
/// </summary>
public partial record OrderLogisticsCompanyId : IGuidStronglyTypedId;

/// <summary>
/// 订单物流公司 (聚合根)
/// </summary>
public class OrderLogisticsCompany : Entity<OrderLogisticsCompanyId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected OrderLogisticsCompany() { }

    /// <summary>名称</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>类型值</summary>
    public int TypeValue { get; private set; }

    /// <summary>排序</summary>
    public int Sort { get; private set; }

    /// <summary>创建</summary>
    public static OrderLogisticsCompany Create(string name, int typeValue, int sort)
    {
        return new OrderLogisticsCompany
        {
            Name = name,
            TypeValue = typeValue,
            Sort = sort,
        };
    }

    /// <summary>更新</summary>
    public void Update(string name, int typeValue, int sort)
    {
        Name = name;
        TypeValue = typeValue;
        Sort = sort;
    }
}
