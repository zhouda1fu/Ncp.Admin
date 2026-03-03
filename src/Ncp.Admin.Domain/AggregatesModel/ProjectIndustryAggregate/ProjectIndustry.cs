using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;

/// <summary>
/// 项目行业 ID（强类型）
/// </summary>
public partial record ProjectIndustryId : IGuidStronglyTypedId;

/// <summary>
/// 项目行业聚合根：主数据，供项目引用（与客户行业分离）
/// </summary>
public class ProjectIndustry : Entity<ProjectIndustryId>, IAggregateRoot
{
    protected ProjectIndustry() { }

    /// <summary>
    /// 行业名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建项目行业
    /// </summary>
    public ProjectIndustry(string name, int sortOrder = 0)
    {
        Name = name;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新项目行业
    /// </summary>
    public void Update(string name, int sortOrder)
    {
        Name = name;
        SortOrder = sortOrder;
    }
}
