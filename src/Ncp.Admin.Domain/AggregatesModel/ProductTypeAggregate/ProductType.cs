using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;

/// <summary>
/// 产品类型 ID（强类型）
/// </summary>
public partial record ProductTypeId : IGuidStronglyTypedId;

/// <summary>
/// 产品类型聚合根：供产品关联，前端维护
/// </summary>
public class ProductType : Entity<ProductTypeId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected ProductType() { }

    /// <summary>类型名称</summary>
    public string Name { get; private set; } = default!;

    /// <summary>排序（数字越小越靠前）</summary>
    public int SortOrder { get; private set; }

    /// <summary>是否启用</summary>
    public bool Visible { get; private set; } = true;

    /// <summary>
    /// 创建产品类型
    /// </summary>
    public ProductType(string name, int sortOrder = 0, bool visible = true)
    {
        Name = name;
        SortOrder = sortOrder;
        Visible = visible;
    }

    /// <summary>
    /// 更新产品类型
    /// </summary>
    public void Update(string name, int sortOrder, bool visible)
    {
        Name = name;
        SortOrder = sortOrder;
        Visible = visible;
    }
}
