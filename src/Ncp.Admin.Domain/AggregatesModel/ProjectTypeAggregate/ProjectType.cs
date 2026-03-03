using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;

/// <summary>
/// 项目类型 ID（强类型）
/// </summary>
public partial record ProjectTypeId : IGuidStronglyTypedId;

/// <summary>
/// 项目类型聚合根：主数据，供项目引用（如小型/中型/大型）
/// </summary>
public class ProjectType : Entity<ProjectTypeId>, IAggregateRoot
{
    protected ProjectType() { }

    /// <summary>
    /// 类型名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建项目类型
    /// </summary>
    public ProjectType(string name, int sortOrder = 0)
    {
        Name = name;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新项目类型
    /// </summary>
    public void Update(string name, int sortOrder)
    {
        Name = name;
        SortOrder = sortOrder;
    }
}
