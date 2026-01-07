using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 组织架构查询DTO
/// </summary>
public record OrganizationUnitQueryDto(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId ParentId, int SortOrder, bool IsActive, DateTimeOffset CreatedAt, DeletedTime? DeletedAt);

/// <summary>
/// 组织架构查询输入参数
/// </summary>
public class OrganizationUnitQueryInput
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public OrganizationUnitId? ParentId { get; set; }
}

/// <summary>
/// 组织架构树形DTO - 应用层数据传输对象
/// </summary>
public record OrganizationUnitTreeDto(
    OrganizationUnitId Id,
    string Name,
    string Description,
    OrganizationUnitId ParentId,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IEnumerable<OrganizationUnitTreeDto> Children);

/// <summary>
/// 组织架构查询服务
/// </summary>
public class OrganizationUnitQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<OrganizationUnit> OrganizationUnitSet { get; } = applicationDbContext.OrganizationUnits;

    /// <summary>
    /// 检查组织架构名称是否存在
    /// </summary>
    public async Task<bool> DoesOrganizationUnitExist(string name, CancellationToken cancellationToken)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .AnyAsync(ou => ou.Name == name, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 检查组织架构ID是否存在
    /// </summary>
    public async Task<bool> DoesOrganizationUnitExist(OrganizationUnitId id, CancellationToken cancellationToken)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .AnyAsync(ou => ou.Id == id, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 根据ID获取组织架构
    /// </summary>
    public async Task<OrganizationUnitQueryDto?> GetOrganizationUnitByIdAsync(OrganizationUnitId id, CancellationToken cancellationToken = default)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .Where(ou => ou.Id == id)
            .Select(ou => new OrganizationUnitQueryDto(ou.Id, ou.Name, ou.Description, ou.ParentId, ou.SortOrder, ou.IsActive, ou.CreatedAt, ou.DeletedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 获取所有组织架构
    /// </summary>
    public async Task<IEnumerable<OrganizationUnitQueryDto>> GetAllOrganizationUnitsAsync(OrganizationUnitQueryInput query, CancellationToken cancellationToken)
    {
        return await OrganizationUnitSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), r => r.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Description), r => r.Description.Contains(query.Description!))
            .WhereIf(query.IsActive.HasValue, r => r.IsActive == query.IsActive)
            .WhereIf(query.ParentId != null, r => r.ParentId == query.ParentId)
            .OrderBy(ou => ou.SortOrder)
            .Select(ou => new OrganizationUnitQueryDto(ou.Id, ou.Name, ou.Description, ou.ParentId, ou.SortOrder, ou.IsActive, ou.CreatedAt, ou.DeletedAt))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取组织架构树
    /// </summary>
    public async Task<IEnumerable<OrganizationUnitTreeDto>> GetOrganizationUnitTreeAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var allOrganizations = await OrganizationUnitSet.AsNoTracking()
            .ToListAsync(cancellationToken);

        // 构建树形结构
        var treeStructure = BuildTreeStructure(allOrganizations, includeInactive);

        // 转换为应用层DTO
        return treeStructure.Select(ou => ConvertToTreeDto(ou));
    }

    /// <summary>
    /// 构建组织架构树形结构
    /// </summary>
    private static IEnumerable<OrganizationUnit> BuildTreeStructure(
        IEnumerable<OrganizationUnit> allOrganizations,
        bool includeInactive = false)
    {
        var organizationDict = allOrganizations.ToDictionary(ou => ou.Id);
        var result = new List<OrganizationUnit>();

        foreach (var org in allOrganizations)
        {
            if (!includeInactive && !org.IsActive)
                continue;

            // 只处理根节点（ParentId为0）
            if (org.ParentId == new OrganizationUnitId(0))
            {
                result.Add(BuildTreeDto(org, organizationDict, includeInactive));
            }
        }

        return result.OrderBy(ou => ou.SortOrder);
    }

    /// <summary>
    /// 构建单个组织架构的树形结构
    /// </summary>
    private static OrganizationUnit BuildTreeDto(
        OrganizationUnit organizationUnit,
        Dictionary<OrganizationUnitId, OrganizationUnit> allOrganizations,
        bool includeInactive)
    {
        var children = new List<OrganizationUnit>();

        // 查找所有以当前组织架构为父级的子组织架构
        var childOrganizations = allOrganizations.Values
            .Where(ou => ou.ParentId == organizationUnit.Id)
            .OrderBy(ou => ou.SortOrder);

        foreach (var child in childOrganizations)
        {
            if (!includeInactive && !child.IsActive)
                continue;

            children.Add(BuildTreeDto(child, allOrganizations, includeInactive));
        }

        // 设置子组织架构
        organizationUnit.Children.Clear();
        foreach (var child in children)
        {
            organizationUnit.Children.Add(child);
        }

        return organizationUnit;
    }

    /// <summary>
    /// 将单个组织架构领域模型转换为树形DTO
    /// </summary>
    private static OrganizationUnitTreeDto ConvertToTreeDto(OrganizationUnit organizationUnit)
    {
        var children = organizationUnit.Children
            .OrderBy(ou => ou.SortOrder)
            .Select(ou => ConvertToTreeDto(ou))
            .ToList();

        return new OrganizationUnitTreeDto(
            organizationUnit.Id,
            organizationUnit.Name,
            organizationUnit.Description,
            organizationUnit.ParentId,
            organizationUnit.SortOrder,
            organizationUnit.IsActive,
            organizationUnit.CreatedAt,
            children
        );
    }
}

