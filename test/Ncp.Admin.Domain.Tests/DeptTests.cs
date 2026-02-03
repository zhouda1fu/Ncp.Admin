using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.Tests;

/// <summary>
/// 部门聚合根（Dept）的领域逻辑单元测试。
/// 覆盖部门信息更新、激活/停用、软删除、子部门管理等业务规则。
/// </summary>
public class DeptTests
{
    /// <summary>
    /// 创建部门时，应正确设置名称、备注、上级部门ID和状态。
    /// </summary>
    [Fact]
    public void Constructor_WithValidArgs_ShouldSetProperties()
    {
        var parentId = new DeptId(0);
        var dept = new Dept("研发部", "技术研发", parentId, 1);

        Assert.Equal("研发部", dept.Name);
        Assert.Equal("技术研发", dept.Remark);
        Assert.Equal(parentId, dept.ParentId);
        Assert.Equal(1, dept.Status);
        Assert.False(dept.IsDeleted);
    }

    /// <summary>
    /// 更新部门信息时，应更新名称、备注、上级部门ID和状态，并更新 UpdateTime。
    /// </summary>
    [Fact]
    public void UpdateInfo_WithValidArgs_ShouldUpdateProperties()
    {
        var parentId = new DeptId(0);
        var newParentId = new DeptId(100);
        var dept = new Dept("研发部", "技术研发", parentId, 1);

        dept.UpdateInfo("研发中心", "核心技术部门", newParentId, 1);

        Assert.Equal("研发中心", dept.Name);
        Assert.Equal("核心技术部门", dept.Remark);
        Assert.Equal(newParentId, dept.ParentId);
        Assert.Equal(1, dept.Status);
    }

    /// <summary>
    /// 激活已停用的部门时，状态应变为 1（启用）。
    /// </summary>
    [Fact]
    public void Activate_WhenDeactivated_ShouldSetStatusToOne()
    {
        var dept = new Dept("研发部", "备注", new DeptId(0), 0);

        dept.Activate();

        Assert.Equal(1, dept.Status);
    }

    /// <summary>
    /// 激活已经处于激活状态的部门时，应抛出已知异常。
    /// </summary>
    [Fact]
    public void Activate_WhenAlreadyActivated_ShouldThrow()
    {
        var dept = new Dept("研发部", "备注", new DeptId(0), 1);

        var ex = Assert.Throws<KnownException>(() => dept.Activate());

        Assert.NotNull(ex.Message);
        Assert.Contains("已经", ex.Message);
    }

    /// <summary>
    /// 停用已激活的部门时，状态应变为 0（禁用）。
    /// </summary>
    [Fact]
    public void Deactivate_WhenActivated_ShouldSetStatusToZero()
    {
        var dept = new Dept("研发部", "备注", new DeptId(0), 1);

        dept.Deactivate();

        Assert.Equal(0, dept.Status);
    }

    /// <summary>
    /// 停用已经处于停用状态的部门时，应抛出已知异常。
    /// </summary>
    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_ShouldThrow()
    {
        var dept = new Dept("研发部", "备注", new DeptId(0), 0);

        var ex = Assert.Throws<KnownException>(() => dept.Deactivate());

        Assert.NotNull(ex.Message);
        Assert.Contains("已经", ex.Message);
    }

    /// <summary>
    /// 软删除未删除的部门时，IsDeleted 应变为 true。
    /// </summary>
    [Fact]
    public void SoftDelete_WhenNotDeleted_ShouldSetIsDeleted()
    {
        var dept = new Dept("研发部", "备注", new DeptId(0), 1);

        dept.SoftDelete();

        Assert.True(dept.IsDeleted);
    }

    /// <summary>
    /// 对已软删除的部门再次软删除时，应抛出已知异常。
    /// </summary>
    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldThrow()
    {
        var dept = new Dept("研发部", "备注", new DeptId(0), 1);
        dept.SoftDelete();

        var ex = Assert.Throws<KnownException>(() => dept.SoftDelete());

        Assert.NotNull(ex.Message);
        Assert.Contains("已经", ex.Message);
    }

    /// <summary>
    /// 添加子部门时，Children 集合应包含该子部门。
    /// </summary>
    [Fact]
    public void AddChild_WithValidChild_ShouldAddToChildren()
    {
        var parent = new Dept("总部", "根", new DeptId(0), 1);
        var child = new Dept("分公司", "子部门", new DeptId(0), 1);

        parent.AddChild(child);

        Assert.Single(parent.Children);
        Assert.Same(child, parent.Children.First());
    }

    /// <summary>
    /// 添加 null 子部门时，应抛出已知异常。
    /// </summary>
    [Fact]
    public void AddChild_WithNull_ShouldThrow()
    {
        var parent = new Dept("总部", "根", new DeptId(0), 1);

        var ex = Assert.Throws<KnownException>(() => parent.AddChild(null!));

        Assert.NotNull(ex.Message);
        Assert.Contains("不能为空", ex.Message);
    }

    /// <summary>
    /// 移除已添加的子部门时，Children 应不再包含该子部门。
    /// </summary>
    [Fact]
    public void RemoveChild_WithExistingChild_ShouldRemoveFromChildren()
    {
        var parent = new Dept("总部", "根", new DeptId(0), 1);
        var child = new Dept("分公司", "子部门", new DeptId(0), 1);
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.Empty(parent.Children);
    }

    /// <summary>
    /// 移除 null 子部门时，应抛出已知异常。
    /// </summary>
    [Fact]
    public void RemoveChild_WithNull_ShouldThrow()
    {
        var parent = new Dept("总部", "根", new DeptId(0), 1);

        var ex = Assert.Throws<KnownException>(() => parent.RemoveChild(null!));

        Assert.NotNull(ex.Message);
        Assert.Contains("不能为空", ex.Message);
    }

    /// <summary>
    /// 获取所有子部门时，应返回当前子部门及其递归子部门（深度优先）。
    /// </summary>
    [Fact]
    public void GetAllChildren_WithNestedChildren_ShouldReturnAllDescendants()
    {
        var root = new Dept("根", "根", new DeptId(0), 1);
        var level1 = new Dept("一级", "备注", new DeptId(0), 1);
        var level2 = new Dept("二级", "备注", new DeptId(0), 1);
        root.AddChild(level1);
        level1.AddChild(level2);

        var all = root.GetAllChildren().ToList();

        Assert.Equal(2, all.Count);
        Assert.Contains(level1, all);
        Assert.Contains(level2, all);
    }

    /// <summary>
    /// 无子部门时，GetAllChildren 应返回空集合。
    /// </summary>
    [Fact]
    public void GetAllChildren_WithNoChildren_ShouldReturnEmpty()
    {
        var dept = new Dept("叶子", "备注", new DeptId(0), 1);

        var all = dept.GetAllChildren().ToList();

        Assert.Empty(all);
    }

    /// <summary>
    /// GetPath 应返回部门名称（当前实现为单层路径）。
    /// </summary>
    [Fact]
    public void GetPath_ShouldReturnName()
    {
        var dept = new Dept("研发部", "备注", new DeptId(0), 1);

        var path = dept.GetPath();

        Assert.Equal("研发部", path);
    }
}
