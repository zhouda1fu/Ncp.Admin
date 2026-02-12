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
        return await PositionSet.AsNoTracking()
            .Where(p => p.Id == id)
            .Join(applicationDbContext.Depts.AsNoTracking(),
                p => p.DeptId, d => d.Id,
                (p, d) => new PositionQueryDto(p.Id, p.Name, p.Code, p.Description, p.DeptId, d.Name, p.SortOrder, p.Status, p.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
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

        var items = await queryable
            .OrderBy(p => p.SortOrder)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Join(applicationDbContext.Depts.AsNoTracking(),
                p => p.DeptId, d => d.Id,
                (p, d) => new PositionQueryDto(p.Id, p.Name, p.Code, p.Description, p.DeptId, d.Name, p.SortOrder, p.Status, p.CreatedAt))
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <summary>
    /// 获取所有岗位
    /// </summary>
    public async Task<IEnumerable<PositionQueryDto>> GetAllPositionsAsync(CancellationToken cancellationToken)
    {
        return await PositionSet.AsNoTracking()
            .Where(p => p.Status == 1)
            .OrderBy(p => p.SortOrder)
            .Join(applicationDbContext.Depts.AsNoTracking(),
                p => p.DeptId, d => d.Id,
                (p, d) => new PositionQueryDto(p.Id, p.Name, p.Code, p.Description, p.DeptId, d.Name, p.SortOrder, p.Status, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
