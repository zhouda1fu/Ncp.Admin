using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;

/// <summary>
/// 行业ID（强类型ID）
/// </summary>
public partial record IndustryId : IGuidStronglyTypedId;

/// <summary>
/// 行业聚合根：层级行业主数据，供客户所属行业多选
/// </summary>
public class Industry : Entity<IndustryId>, IAggregateRoot
{
    protected Industry() { }

    /// <summary>
    /// 行业名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 父级行业ID（ null 表示一级）
    /// </summary>
    public IndustryId? ParentId { get; private set; }
    /// <summary>
    /// 排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; private set; }

    /// <summary>
    /// 创建行业
    /// </summary>
    public Industry(string name, IndustryId? parentId, int sortOrder = 0, string? remark = null)
    {
        Name = name ?? string.Empty;
        ParentId = parentId;
        SortOrder = sortOrder;
        Remark = remark;
    }

    /// <summary>
    /// 更新行业信息
    /// </summary>
    public void Update(string name, int sortOrder, string? remark = null)
    {
        Name = name ?? string.Empty;
        SortOrder = sortOrder;
        Remark = remark;
    }
}
