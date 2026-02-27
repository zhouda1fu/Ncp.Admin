using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;

/// <summary>
/// 客户来源 ID（强类型）。
/// </summary>
public partial record CustomerSourceId : IGuidStronglyTypedId;

/// <summary>
/// 客户来源聚合根：主数据，仅 id + 名称，供用户维护、客户档案引用。
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
    /// 创建客户来源。
    /// </summary>
    public CustomerSource(string name, int sortOrder = 0)
    {
        Name = name ?? string.Empty;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新客户来源。
    /// </summary>
    public void Update(string name, int sortOrder)
    {
        Name = name ?? string.Empty;
        SortOrder = sortOrder;
    }
}
