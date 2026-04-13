using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.OrderLogisticsMethodAggregate;

/// <summary>
/// 订单物流方式 ID (强类型)
/// </summary>
public partial record OrderLogisticsMethodId : IGuidStronglyTypedId;

/// <summary>
/// 订单物流方式 (聚合根)
/// </summary>
public class OrderLogisticsMethod : Entity<OrderLogisticsMethodId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected OrderLogisticsMethod() { }

    /// <summary>名称</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>类型值</summary>
    public int TypeValue { get; private set; }

    /// <summary>排序</summary>
    public int Sort { get; private set; }

    /// <summary>新建（业务创建；EF 使用无参 <see cref="OrderLogisticsMethod" />）</summary>
    /// <param name="name">名称</param>
    /// <param name="typeValue">类型值</param>
    /// <param name="sort">排序</param>
    public OrderLogisticsMethod(string name, int typeValue, int sort)
    {
        Name = name;
        TypeValue = typeValue;
        Sort = sort;
    }

    /// <summary>更新</summary>
    public void Update(string name, int typeValue, int sort)
    {
        Name = name;
        TypeValue = typeValue;
        Sort = sort;
    }
}

