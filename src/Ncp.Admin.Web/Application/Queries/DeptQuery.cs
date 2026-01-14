using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 部门查询DTO
/// </summary>
public record DeptQueryDto(DeptId Id, string Name, string Remark, DeptId ParentId, int Status, DateTimeOffset CreatedAt, DeletedTime? DeletedAt);

/// <summary>
/// 部门查询输入参数
/// </summary>
public class DeptQueryInput
{
    public string? Name { get; set; }
    public string? Remark { get; set; }
    public int? Status { get; set; }
    public DeptId? ParentId { get; set; }
}

/// <summary>
/// 部门树形DTO - 应用层数据传输对象
/// </summary>
public record DeptTreeDto(
    DeptId Id,
    string Name,
    string Remark,
    DeptId ParentId,
    int Status,
    DateTimeOffset CreatedAt,
    IEnumerable<DeptTreeDto> Children);

/// <summary>
/// 部门查询服务
/// </summary>
public class DeptQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<Dept> DeptSet { get; } = applicationDbContext.Depts;

    /// <summary>
    /// 检查部门名称是否存在
    /// </summary>
    public async Task<bool> DoesDeptExist(string name, CancellationToken cancellationToken)
    {
        return await DeptSet.AsNoTracking()
            .AnyAsync(d => d.Name == name, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 检查部门ID是否存在
    /// </summary>
    public async Task<bool> DoesDeptExist(DeptId id, CancellationToken cancellationToken)
    {
        return await DeptSet.AsNoTracking()
            .AnyAsync(d => d.Id == id, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 根据ID获取部门
    /// </summary>
    public async Task<DeptQueryDto?> GetDeptByIdAsync(DeptId id, CancellationToken cancellationToken = default)
    {
        return await DeptSet.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DeptQueryDto(d.Id, d.Name, d.Remark, d.ParentId, d.Status, d.CreatedAt, d.DeletedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 获取所有部门
    /// </summary>
    public async Task<IEnumerable<DeptQueryDto>> GetAllDeptsAsync(DeptQueryInput query, CancellationToken cancellationToken)
    {
        return await DeptSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), d => d.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Remark), d => d.Remark.Contains(query.Remark!))
            .WhereIf(query.Status.HasValue, d => d.Status == query.Status)
            .WhereIf(query.ParentId != null, d => d.ParentId == query.ParentId)
            .OrderBy(d => d.CreatedAt)
            .Select(d => new DeptQueryDto(d.Id, d.Name, d.Remark, d.ParentId, d.Status, d.CreatedAt, d.DeletedAt))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取部门树
    /// </summary>
    public async Task<IEnumerable<DeptTreeDto>> GetDeptTreeAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var allDepts = await DeptSet.AsNoTracking()
            .ToListAsync(cancellationToken);

        // 构建树形结构
        var treeStructure = BuildTreeStructure(allDepts, includeInactive);

        // 转换为应用层DTO
        return treeStructure.Select(d => ConvertToTreeDto(d));
    }

    /// <summary>
    /// 构建部门树形结构
    /// </summary>
    private static IEnumerable<Dept> BuildTreeStructure(
        IEnumerable<Dept> allDepts,
        bool includeInactive = false)
    {
        var deptDict = allDepts.ToDictionary(d => d.Id);
        var result = new List<Dept>();

        foreach (var dept in allDepts)
        {
            if (!includeInactive && dept.Status == 0)
                continue;

            // 只处理根节点（ParentId为0）
            if (dept.ParentId == new DeptId(0))
            {
                result.Add(BuildTreeDto(dept, deptDict, includeInactive));
            }
        }

        return result.OrderBy(d => d.CreatedAt);
    }

    /// <summary>
    /// 构建单个部门的树形结构
    /// </summary>
    private static Dept BuildTreeDto(
        Dept dept,
        Dictionary<DeptId, Dept> allDepts,
        bool includeInactive)
    {
        var children = new List<Dept>();

        // 查找所有以当前部门为父级的子部门
        var childDepts = allDepts.Values
            .Where(d => d.ParentId == dept.Id)
            .OrderBy(d => d.CreatedAt);

        foreach (var child in childDepts)
        {
            if (!includeInactive && child.Status == 0)
                continue;

            children.Add(BuildTreeDto(child, allDepts, includeInactive));
        }

        // 设置子部门
        dept.Children.Clear();
        foreach (var child in children)
        {
            dept.Children.Add(child);
        }

        return dept;
    }

    /// <summary>
    /// 将单个部门领域模型转换为树形DTO
    /// </summary>
    private static DeptTreeDto ConvertToTreeDto(Dept dept)
    {
        var children = dept.Children
            .OrderBy(d => d.CreatedAt)
            .Select(d => ConvertToTreeDto(d))
            .ToList();

        return new DeptTreeDto(
            dept.Id,
            dept.Name,
            dept.Remark,
            dept.ParentId,
            dept.Status,
            dept.CreatedAt,
            children
        );
    }
}
