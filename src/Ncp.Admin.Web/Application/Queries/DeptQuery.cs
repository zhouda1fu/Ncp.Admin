using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Services;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 部门查询DTO
/// </summary>
public record DeptResponsibleUserQueryDto(UserId UserId, string Name, bool IsDefault, int SortOrder);

/// <summary>
/// 部门查询DTO
/// </summary>
public record DeptQueryDto(
    DeptId Id,
    string Name,
    string Remark,
    DeptId ParentId,
    IReadOnlyList<DeptResponsibleUserQueryDto> ResponsibleUsers,
    int Status,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DeletedTime? DeletedAt);

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
    IReadOnlyList<DeptResponsibleUserQueryDto> ResponsibleUsers,
    int Status,
    int SortOrder,
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
        var dept = await DeptSet.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new { d.Id, d.Name, d.Remark, d.ParentId, d.Status, d.SortOrder, d.CreatedAt, d.DeletedAt })
            .FirstOrDefaultAsync(cancellationToken);
        if (dept == null)
        {
            return null;
        }

        var responsibleUsers = await GetResponsibleUsersByDeptIdsAsync([dept.Id], cancellationToken);
        return new DeptQueryDto(
            dept.Id,
            dept.Name,
            dept.Remark,
            dept.ParentId,
            responsibleUsers.GetValueOrDefault(dept.Id, []),
            dept.Status,
            dept.SortOrder,
            dept.CreatedAt,
            dept.DeletedAt);
    }

    /// <summary>
    /// 按部门名称精确匹配；若存在多条同名部门返回 null（由调用方提示不唯一）。
    /// </summary>
    public async Task<DeptQueryDto?> GetDeptByExactNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmed = name.Trim();
        var matches = await DeptSet.AsNoTracking()
            .Where(d => d.Name == trimmed)
            .Select(d => new { d.Id, d.Name, d.Remark, d.ParentId, d.Status, d.SortOrder, d.CreatedAt, d.DeletedAt })
            .ToListAsync(cancellationToken);
        if (matches.Count != 1)
        {
            return null;
        }

        var match = matches[0];
        var responsibleUsers = await GetResponsibleUsersByDeptIdsAsync([match.Id], cancellationToken);
        return new DeptQueryDto(
            match.Id,
            match.Name,
            match.Remark,
            match.ParentId,
            responsibleUsers.GetValueOrDefault(match.Id, []),
            match.Status,
            match.SortOrder,
            match.CreatedAt,
            match.DeletedAt);
    }

    /// <summary>
    /// 获取所有部门
    /// </summary>
    public async Task<IEnumerable<DeptQueryDto>> GetAllDeptsAsync(DeptQueryInput query, CancellationToken cancellationToken)
    {
        var depts = await DeptSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), d => d.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Remark), d => d.Remark.Contains(query.Remark!))
            .WhereIf(query.Status.HasValue, d => d.Status == query.Status)
            .WhereIf(query.ParentId != null, d => d.ParentId == query.ParentId)
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.CreatedAt)
            .Select(d => new { d.Id, d.Name, d.Remark, d.ParentId, d.Status, d.SortOrder, d.CreatedAt, d.DeletedAt })
            .ToListAsync(cancellationToken);
        var responsibleUsers = await GetResponsibleUsersByDeptIdsAsync(depts.Select(d => d.Id).ToList(), cancellationToken);
        return depts.Select(d => new DeptQueryDto(
            d.Id,
            d.Name,
            d.Remark,
            d.ParentId,
            responsibleUsers.GetValueOrDefault(d.Id, []),
            d.Status,
            d.SortOrder,
            d.CreatedAt,
            d.DeletedAt));
    }

    /// <summary>
    /// 获取部门树
    /// 优化：使用投影只选择需要的字段，减少内存占用
    /// </summary>
    public async Task<IEnumerable<DeptTreeDto>> GetDeptTreeAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        // 使用投影只选择构建树所需的字段，减少内存占用
        var allDepts = await DeptSet.AsNoTracking()
            .WhereIf(!includeInactive, d => d.Status != 0)
            .Select(d => new DeptTreeNode
            {
                Id = d.Id,
                Name = d.Name,
                Remark = d.Remark,
                ParentId = d.ParentId,
                Status = d.Status,
                SortOrder = d.SortOrder,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);
        var responsibleUsers = await GetResponsibleUsersByDeptIdsAsync(allDepts.Select(d => d.Id).ToList(), cancellationToken);
        foreach (var dept in allDepts)
        {
            dept.ResponsibleUsers = responsibleUsers.GetValueOrDefault(dept.Id, []);
        }

        // 构建树形结构
        var treeStructure = BuildTreeStructureFromProjection(allDepts);

        // 转换为应用层DTO
        return treeStructure.Select(d => ConvertToTreeDtoFromProjection(d));
    }

    /// <summary>
    /// 构建部门树形结构（基于投影数据）
    /// </summary>
    private static List<DeptTreeNode> BuildTreeStructureFromProjection(
        List<DeptTreeNode> allDepts)
    {
        var deptDict = allDepts.ToDictionary(d => d.Id);
        var result = new List<DeptTreeNode>();

        foreach (var dept in allDepts)
        {
            // 只处理根节点（ParentId为0）
            if (dept.ParentId == DeptId.Unassigned)
            {
                result.Add(BuildTreeDtoFromProjection(dept, deptDict));
            }
        }

        return result.OrderBy(d => d.SortOrder).ThenBy(d => d.CreatedAt).ToList();
    }

    /// <summary>
    /// 构建单个部门的树形结构（基于投影数据）
    /// </summary>
    private static DeptTreeNode BuildTreeDtoFromProjection(
        DeptTreeNode dept,
        Dictionary<DeptId, DeptTreeNode> allDepts)
    {
        var children = new List<DeptTreeNode>();

        // 查找所有以当前部门为父级的子部门
        var childDepts = allDepts.Values
            .Where(d => d.ParentId == dept.Id)
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.CreatedAt);

        foreach (var child in childDepts)
        {
            children.Add(BuildTreeDtoFromProjection(child, allDepts));
        }

        return new DeptTreeNode
        {
            Id = dept.Id,
            Name = dept.Name,
            Remark = dept.Remark,
            ParentId = dept.ParentId,
            ResponsibleUsers = dept.ResponsibleUsers,
            Status = dept.Status,
            SortOrder = dept.SortOrder,
            CreatedAt = dept.CreatedAt,
            Children = children
        };
    }

    /// <summary>
    /// 获取指定部门及其所有子部门的 ID 列表
    /// </summary>
    public async Task<List<DeptId>> GetAllChildDeptIdsAsync(DeptId parentDeptId, CancellationToken cancellationToken = default)
    {
        var allDepts = await DeptSet.AsNoTracking()
            .Select(d => new { d.Id, d.ParentId })
            .ToListAsync(cancellationToken);

        var result = new List<DeptId> { parentDeptId };
        AddChildIdsRecursive(parentDeptId, allDepts.Select(d => (d.Id, d.ParentId)).ToList(), result);
        return result;
    }

    /// <summary>名为「营销中心」的部门及其全部下级部门 ID（含根）。名称不存在时返回空列表。</summary>
    public async Task<IReadOnlyList<DeptId>> GetMarketingCenterSubtreeDeptIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var root = await GetDeptByExactNameAsync("营销中心", cancellationToken);
        if (root is null)
            return Array.Empty<DeptId>();
        return await GetAllChildDeptIdsAsync(root.Id, cancellationToken);
    }

    /// <summary>部门名称是否包含「营销」（客户公海片区分配人员范围口径）。</summary>
    public async Task<bool> IsMarketingDeptByNameAsync(DeptId deptId, CancellationToken cancellationToken = default)
    {
        var deptName = await DeptSet.AsNoTracking()
            .Where(d => d.Id == deptId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync(cancellationToken);
        return !string.IsNullOrWhiteSpace(deptName) && deptName.Contains("营销", StringComparison.Ordinal);
    }

    /// <summary>名为「事务部」的部门及其全部下级部门 ID（含根）。名称不存在时返回空列表。</summary>
    public async Task<IReadOnlyList<DeptId>> GetAffairsDeptSubtreeDeptIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var root = await GetDeptByExactNameAsync("事务部", cancellationToken);
        if (root is null)
            return Array.Empty<DeptId>();
        return await GetAllChildDeptIdsAsync(root.Id, cancellationToken);
    }

    /// <summary>名为「技术部」的部门及其全部下级部门 ID（含根）。名称不存在时返回空列表。</summary>
    public async Task<IReadOnlyList<DeptId>> GetTechnologyDeptSubtreeDeptIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var root = await GetDeptByExactNameAsync("技术部", cancellationToken);
        if (root is null)
            return Array.Empty<DeptId>();
        return await GetAllChildDeptIdsAsync(root.Id, cancellationToken);
    }

    /// <summary>名为「产品研发中心」的部门及其全部下级部门 ID（含根）。名称不存在时返回空列表。</summary>
    public async Task<IReadOnlyList<DeptId>> GetProductResearchCenterSubtreeDeptIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var root = await GetDeptByExactNameAsync("产品研发中心", cancellationToken);
        if (root is null)
            return Array.Empty<DeptId>();
        return await GetAllChildDeptIdsAsync(root.Id, cancellationToken);
    }

    /// <summary>
    /// 值日安排可选部门：「产品研发中心」「网络推广组」及其各自下级部门 ID（去重）。
    /// </summary>
    public async Task<IReadOnlyList<DeptId>> GetDutyAllowedDeptSubtreeIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new List<DeptId>();

        var productResearchIds = await GetProductResearchCenterSubtreeDeptIdsAsync(cancellationToken);
        result.AddRange(productResearchIds);

        var networkPromoRoot = await GetDeptByExactNameAsync("网络推广组", cancellationToken);
        if (networkPromoRoot is not null)
        {
            var networkPromoIds = await GetAllChildDeptIdsAsync(networkPromoRoot.Id, cancellationToken);
            result.AddRange(networkPromoIds);
        }

        return result.Distinct().ToList();
    }

    /// <summary>名为「仓储物流部」的部门及其全部下级部门 ID（含根）。名称不存在时返回空列表。</summary>
    public async Task<IReadOnlyList<DeptId>> GetWarehouseDeptSubtreeDeptIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var root = await GetDeptByExactNameAsync("仓储物流部", cancellationToken);
        if (root is null)
            return Array.Empty<DeptId>();
        return await GetAllChildDeptIdsAsync(root.Id, cancellationToken);
    }

    /// <summary>名为「财务部」的部门及其全部下级部门 ID（含根）。名称不存在时返回空列表。</summary>
    public async Task<IReadOnlyList<DeptId>> GetFinanceDeptSubtreeDeptIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var root = await GetDeptByExactNameAsync("财务部", cancellationToken);
        if (root is null)
            return Array.Empty<DeptId>();
        return await GetAllChildDeptIdsAsync(root.Id, cancellationToken);
    }

    private static void AddChildIdsRecursive(DeptId parentId, List<(DeptId Id, DeptId ParentId)> allDepts, List<DeptId> result)
    {
        var children = allDepts.Where(d => d.ParentId == parentId).Select(d => d.Id).ToList();
        foreach (var childId in children)
        {
            result.Add(childId);
            AddChildIdsRecursive(childId, allDepts, result);
        }
    }

    /// <summary>
    /// 将投影节点转换为树形DTO
    /// </summary>
    private static DeptTreeDto ConvertToTreeDtoFromProjection(DeptTreeNode node)
    {
        var children = node.Children
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.CreatedAt)
            .Select(d => ConvertToTreeDtoFromProjection(d))
            .ToList();

        return new DeptTreeDto(
            node.Id,
            node.Name,
            node.Remark,
            node.ParentId,
            node.ResponsibleUsers,
            node.Status,
            node.SortOrder,
            node.CreatedAt,
            children
        );
    }

    /// <summary>
    /// 批量加载部门负责人，并补齐负责人展示名，供部门详情、部门树和工作流负责人解析共用。
    /// </summary>
    public async Task<IReadOnlyDictionary<DeptId, IReadOnlyList<DeptResponsibleUserQueryDto>>> GetResponsibleUsersByDeptIdsAsync(
        IReadOnlyList<DeptId> deptIds,
        CancellationToken cancellationToken = default)
    {
        if (deptIds.Count == 0)
        {
            return new Dictionary<DeptId, IReadOnlyList<DeptResponsibleUserQueryDto>>();
        }

        var rows = await applicationDbContext.DeptResponsibleUsers.AsNoTracking()
            .Where(x => deptIds.Contains(x.DeptId))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new { x.DeptId, x.UserId, x.IsDefault, x.SortOrder })
            .ToListAsync(cancellationToken);
        if (rows.Count == 0)
        {
            return new Dictionary<DeptId, IReadOnlyList<DeptResponsibleUserQueryDto>>();
        }

        var userIds = rows.Select(x => x.UserId).Distinct().ToList();
        var users = await applicationDbContext.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.RealName, u.Name })
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.RealName) ? u.Name : u.RealName, cancellationToken);

        return rows
            .GroupBy(x => x.DeptId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<DeptResponsibleUserQueryDto>)g
                    .Select(x => new DeptResponsibleUserQueryDto(
                        x.UserId,
                        users.GetValueOrDefault(x.UserId, string.Empty),
                        x.IsDefault,
                        x.SortOrder))
                    .ToList());
    }

    /// <summary>
    /// 部门树节点（用于内存中的树构建）
    /// </summary>
    private sealed class DeptTreeNode
    {
        public DeptId Id { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public DeptId ParentId { get; set; } = default!;
        public IReadOnlyList<DeptResponsibleUserQueryDto> ResponsibleUsers { get; set; } = [];
        public int Status { get; set; }
        public int SortOrder { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<DeptTreeNode> Children { get; set; } = new();
    }

    /// <summary>
    /// 构建流程定义导入时的部门 ID 重映射索引（按部门名称；重名部门仅保留首个匹配）。
    /// </summary>
    public async Task<WorkflowRemapDeptIndex> BuildWorkflowRemapDeptIndexAsync(CancellationToken cancellationToken = default)
    {
        var rows = await DeptSet.AsNoTracking()
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(cancellationToken);

        var index = new WorkflowRemapDeptIndex();
        foreach (var row in rows)
        {
            index.Add(row.Id, row.Name);
        }

        return index;
    }
}
