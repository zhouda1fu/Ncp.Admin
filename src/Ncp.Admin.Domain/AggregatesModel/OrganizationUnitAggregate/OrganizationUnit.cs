using System.ComponentModel.DataAnnotations.Schema;
using Ncp.Admin.Domain.DomainEvents.OrganizationUnitEvents;

namespace Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;

/// <summary>
/// 组织架构ID（强类型ID）
/// </summary>
public partial record OrganizationUnitId : IInt64StronglyTypedId;

/// <summary>
/// 组织架构聚合根
/// 用于管理企业组织架构的层级结构
/// </summary>
public class OrganizationUnit : Entity<OrganizationUnitId>, IAggregateRoot
{
    /// <summary>
    /// 组织架构名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 上级组织架构ID
    /// </summary>
    public OrganizationUnitId ParentId { get; private set; } = default!;

    /// <summary>
    /// 排序
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 是否删除
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 删除时间
    /// </summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 子组织架构（不映射到数据库，用于内存中的树形结构）
    /// </summary>
    [NotMapped]
    public virtual ICollection<OrganizationUnit> Children { get; } = [];

    protected OrganizationUnit()
    {
    }

    /// <summary>
    /// 创建组织架构
    /// </summary>
    /// <param name="name">组织架构名称</param>
    /// <param name="description">备注</param>
    /// <param name="parentId">上级组织架构ID</param>
    /// <param name="sortOrder">排序</param>
    public OrganizationUnit(string name, string description, OrganizationUnitId parentId, int sortOrder)
    {
        CreatedAt = DateTimeOffset.Now;
        Name = name;
        Description = description;
        ParentId = parentId;
        SortOrder = sortOrder;
        IsActive = true;
    }

    /// <summary>
    /// 更新组织架构信息
    /// </summary>
    /// <param name="name">组织架构名称</param>
    /// <param name="description">备注</param>
    /// <param name="parentId">上级组织架构ID</param>
    /// <param name="sortOrder">排序</param>
    public void UpdateInfo(string name, string description, OrganizationUnitId parentId, int sortOrder)
    {
        Name = name;
        Description = description;
        ParentId = parentId;
        SortOrder = sortOrder;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

        AddDomainEvent(new OrganizationUnitInfoChangedDomainEvent(this));
    }

    /// <summary>
    /// 激活组织架构
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            throw new KnownException("组织架构已经是激活状态");
        }

        IsActive = true;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 停用组织架构
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new KnownException("组织架构已经被停用");
        }

        IsActive = false;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 软删除组织架构
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted)
        {
            throw new KnownException("组织架构已经被删除");
        }

        IsDeleted = true;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 添加子组织架构
    /// </summary>
    /// <param name="child">子组织架构</param>
    public void AddChild(OrganizationUnit child)
    {
        if (child == null)
        {
            throw new KnownException("子组织架构不能为空");
        }

        Children.Add(child);
    }

    /// <summary>
    /// 移除子组织架构
    /// </summary>
    /// <param name="child">子组织架构</param>
    public void RemoveChild(OrganizationUnit child)
    {
        if (child == null)
        {
            throw new KnownException("子组织架构不能为空");
        }

        Children.Remove(child);
    }

    /// <summary>
    /// 获取所有子组织架构（包括子级的子级）
    /// </summary>
    /// <returns>所有子组织架构</returns>
    public IEnumerable<OrganizationUnit> GetAllChildren()
    {
        var result = new List<OrganizationUnit>();
        foreach (var child in Children)
        {
            result.Add(child);
            result.AddRange(child.GetAllChildren());
        }
        return result;
    }

    /// <summary>
    /// 获取组织架构层级路径
    /// </summary>
    /// <returns>层级路径</returns>
    public string GetPath()
    {
        return Name;
    }
}

