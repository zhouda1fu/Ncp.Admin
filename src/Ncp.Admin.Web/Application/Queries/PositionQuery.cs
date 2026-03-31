using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 岗位查询DTO
/// </summary>
public record PositionQueryDto(
    PositionId Id,
    string Name,
    string Code,
    string Description,
    DeptId DeptId,
    string? DeptName,
    int SortOrder,
    int Status,
    DateTimeOffset CreatedAt);

/// <summary>
/// 岗位查询输入参数
/// </summary>
public class PositionQueryInput
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public DeptId? DeptId { get; set; }
    public int? Status { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 岗位查询服务
/// </summary>
public class PositionQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<Position> PositionSet { get; } = applicationDbContext.Positions;

    /// <summary>
    /// 检查岗位编码是否存在
    /// </summary>
    public async Task<bool> DoesPositionCodeExist(string code, CancellationToken cancellationToken)
    {
        return await PositionSet.AsNoTracking()
            .AnyAsync(p => p.Code == code, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 检查岗位编码是否存在（排除指定ID）
    /// </summary>
    public async Task<bool> DoesPositionCodeExist(string code, PositionId excludeId, CancellationToken cancellationToken)
    {
        return await PositionSet.AsNoTracking()
            .AnyAsync(p => p.Code == code && p.Id != excludeId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 根据ID获取岗位
    /// </summary>
    public async Task<PositionQueryDto?> GetPositionByIdAsync(PositionId id, CancellationToken cancellationToken = default)
    {
        var position = await PositionSet.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new { p.Id, p.Name, p.Code, p.Description, p.DeptId, p.SortOrder, p.Status, p.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (position == null)
            return null;

        var deptName = await applicationDbContext.Depts.AsNoTracking()
            .Where(d => d.Id == position.DeptId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new PositionQueryDto(position.Id, position.Name, position.Code, position.Description, position.DeptId, deptName, position.SortOrder, position.Status, position.CreatedAt);
    }

    /// <summary>
    /// 获取岗位列表（分页）
    /// </summary>
    public async Task<(IEnumerable<PositionQueryDto> Items, int Total)> GetPositionListAsync(PositionQueryInput query, CancellationToken cancellationToken)
    {
        var queryable = PositionSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), p => p.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Code), p => p.Code.Contains(query.Code!))
            .WhereIf(query.DeptId != null, p => p.DeptId == query.DeptId)
            .WhereIf(query.Status.HasValue, p => p.Status == query.Status);

        var total = await queryable.CountAsync(cancellationToken);

        var positions = await queryable
            .OrderBy(p => p.SortOrder)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new { p.Id, p.Name, p.Code, p.Description, p.DeptId, p.SortOrder, p.Status, p.CreatedAt })
            .ToListAsync(cancellationToken);

        if (positions.Count == 0)
            return ([], total);

        var deptIds = positions.Select(p => p.DeptId).Distinct().ToList();
        var deptNames = await applicationDbContext.Depts.AsNoTracking()
            .Where(d => deptIds.Contains(d.Id))
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(cancellationToken);
        var deptNameMap = deptNames.ToDictionary(d => d.Id, d => d.Name);

        var items = positions
            .Select(p => new PositionQueryDto(p.Id, p.Name, p.Code, p.Description, p.DeptId, deptNameMap.GetValueOrDefault(p.DeptId), p.SortOrder, p.Status, p.CreatedAt));

        return (items, total);
    }

    /// <summary>
    /// 获取所有岗位
    /// </summary>
    /// <summary>
    /// 按岗位名称解析；若指定 <paramref name="deptId"/> 则限定部门，否则仅在全库唯一匹配时返回。
    /// </summary>
    public async Task<PositionQueryDto?> GetPositionByNameForImportAsync(string name, DeptId? deptId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmed = name.Trim();
        var queryable = PositionSet.AsNoTracking().Where(p => p.Name == trimmed);
        if (deptId != null)
        {
            queryable = queryable.Where(p => p.DeptId == deptId);
        }

        var positions = await queryable
            .Select(p => new { p.Id, p.Name, p.Code, p.Description, p.DeptId, p.SortOrder, p.Status, p.CreatedAt })
            .ToListAsync(cancellationToken);
        if (positions.Count == 0)
        {
            return null;
        }

        if (positions.Count > 1)
        {
            return null;
        }

        var p = positions[0];
        var deptName = await applicationDbContext.Depts.AsNoTracking()
            .Where(d => d.Id == p.DeptId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync(cancellationToken);
        return new PositionQueryDto(p.Id, p.Name, p.Code, p.Description, p.DeptId, deptName, p.SortOrder, p.Status, p.CreatedAt);
    }

    public async Task<IEnumerable<PositionQueryDto>> GetAllPositionsAsync(CancellationToken cancellationToken)
    {
        var positions = await PositionSet.AsNoTracking()
            .Where(p => p.Status == 1)
            .OrderBy(p => p.SortOrder)
            .Select(p => new { p.Id, p.Name, p.Code, p.Description, p.DeptId, p.SortOrder, p.Status, p.CreatedAt })
            .ToListAsync(cancellationToken);

        if (positions.Count == 0)
            return [];

        var deptIds = positions.Select(p => p.DeptId).Distinct().ToList();
        var deptNames = await applicationDbContext.Depts.AsNoTracking()
            .Where(d => deptIds.Contains(d.Id))
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(cancellationToken);
        var deptNameMap = deptNames.ToDictionary(d => d.Id, d => d.Name);

        return positions
            .Select(p => new PositionQueryDto(p.Id, p.Name, p.Code, p.Description, p.DeptId, deptNameMap.GetValueOrDefault(p.DeptId), p.SortOrder, p.Status, p.CreatedAt));
    }
}
