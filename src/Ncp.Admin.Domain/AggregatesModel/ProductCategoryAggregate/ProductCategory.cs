using System;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;

/// <summary>
/// 产品分类 ID（强类型）
/// </summary>
public partial record ProductCategoryId : IGuidStronglyTypedId;

/// <summary>
/// 产品分类聚合根：树形结构，供产品关联（根节点 <see cref="ParentId"/> 为 <see cref="RootParentId"/>）
/// </summary>
public class ProductCategory : Entity<ProductCategoryId>, IAggregateRoot
{
    /// <summary>根节点占位父 ID（与 <see cref="Guid.Empty"/> 对应）</summary>
    public static ProductCategoryId RootParentId { get; } = new(Guid.Empty);

    /// <summary>EF/序列化用</summary>
    protected ProductCategory() { }

    /// <summary>分类名称</summary>
    public string Name { get; private set; } = default!;

    /// <summary>备注</summary>
    public string Remark { get; private set; } = default!;

    /// <summary>上级分类 ID（根节点为 <see cref="RootParentId"/>）</summary>
    public ProductCategoryId ParentId { get; private set; } = RootParentId;

    /// <summary>排序（数字越小越靠前）</summary>
    public int SortOrder { get; private set; }

    /// <summary>是否可见/启用</summary>
    public bool Visible { get; private set; } = true;

    /// <summary>是否为优惠分类</summary>
    public bool IsDiscount { get; private set; } = false;

    /// <summary>是否为根节点</summary>
    public bool IsRoot => ParentId == RootParentId;

    /// <summary>
    /// 创建产品分类
    /// </summary>
    public ProductCategory(
        string name,
        string remark,
        ProductCategoryId parentId,
        int sortOrder = 0,
        bool visible = true,
        bool isDiscount = false)
    {
        Name = name;
        Remark = remark;
        ParentId = parentId;
        SortOrder = sortOrder;
        Visible = visible;
        IsDiscount = isDiscount;
    }

    /// <summary>
    /// 更新产品分类
    /// </summary>
    public void Update(
        string name,
        string remark,
        ProductCategoryId parentId,
        int sortOrder,
        bool visible,
        bool isDiscount)
    {
        Name = name;
        Remark = remark;
        ParentId = parentId;
        SortOrder = sortOrder;
        Visible = visible;
        IsDiscount = isDiscount;
    }
}
