using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;

/// <summary>
/// 客户来源使用场景：公海、客户列表、通用（两处都显示）。
/// </summary>
public enum CustomerSourceUsageScene
{
    /// <summary>公海创建客户时可选</summary>
    Sea = 0,

    /// <summary>客户列表新建/编辑时可选</summary>
    List = 1,

    /// <summary>公海与客户列表均可选</summary>
    Both = 2,
}

/// <summary>
/// 客户来源 ID（强类型）。
/// </summary>
public partial record CustomerSourceId : IGuidStronglyTypedId;

/// <summary>
/// 客户来源聚合根：主数据，id + 名称 + 使用场景，供用户维护、客户档案引用。
/// </summary>
public class CustomerSource : Entity<CustomerSourceId>, IAggregateRoot
{
    /// <summary>
    /// EF/序列化用。
    /// </summary>
    protected CustomerSource() { }

    /// <summary>
    /// 来源名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 排序（数字越小越靠前）。
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 使用场景（公海 / 客户列表 / 通用）。
    /// </summary>
    public CustomerSourceUsageScene UsageScene { get; private set; }

    /// <summary>
    /// 创建客户来源。
    /// </summary>
    public CustomerSource(string name, int sortOrder = 0, CustomerSourceUsageScene usageScene = CustomerSourceUsageScene.Both)
    {
        Name = name;
        SortOrder = sortOrder;
        UsageScene = usageScene;
    }

    /// <summary>
    /// 更新客户来源。
    /// </summary>
    public void Update(string name, int sortOrder, CustomerSourceUsageScene usageScene)
    {
        Name = name;
        SortOrder = sortOrder;
        UsageScene = usageScene;
    }
}
