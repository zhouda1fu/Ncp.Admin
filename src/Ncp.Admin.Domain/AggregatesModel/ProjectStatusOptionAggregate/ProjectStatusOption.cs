using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;

/// <summary>
/// 项目状态选项 ID（强类型）
/// </summary>
public partial record ProjectStatusOptionId : IGuidStronglyTypedId;

/// <summary>
/// 项目状态选项聚合根：主数据，供项目引用（如新项目/进行中/已完成）
/// </summary>
public class ProjectStatusOption : Entity<ProjectStatusOptionId>, IAggregateRoot
{
    protected ProjectStatusOption() { }

    /// <summary>
    /// 状态名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 状态编码
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建项目状态选项
    /// </summary>
    public ProjectStatusOption(string name, string code, int sortOrder = 0)
    {
        Name = name;
        Code = code ?? string.Empty;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新项目状态选项
    /// </summary>
    public void Update(string name, string code, int sortOrder)
    {
        Name = name;
        Code = code;
        SortOrder = sortOrder;
    }
}
