using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.RegionAggregate;

/// <summary>
/// 区域 ID（强类型，值为区域码 Code）
/// </summary>
public partial record RegionId : IInt64StronglyTypedId;

/// <summary>
/// 区域聚合根：层级区域主数据（省/市/区等），供客户等引用。
/// </summary>
public class Region : Entity<RegionId>, IAggregateRoot
{
    /// <summary>
    /// EF/序列化用
    /// </summary>
    protected Region() { }

    /// <summary>
    /// 区域名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 父级区域 ID，根节点为 0
    /// </summary>
    public RegionId ParentId { get; private set; } = default!;

    /// <summary>
    /// 层级（0=全国，1=大区，2=省/直辖市，3=市/区等）
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// 同级排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建区域（Id 即区域码，由调用方传入）
    /// </summary>
    public Region(RegionId id, string name, RegionId parentId, int level, int sortOrder = 0)
    {
        Id = id;
        Name = name ;
        ParentId = parentId;
        Level = level;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新区域信息
    /// </summary>
    public void Update(string name, RegionId parentId, int level, int sortOrder = 0)
    {
        Name = name ;
        ParentId = parentId;
        Level = level;
        SortOrder = sortOrder;
    }
}
