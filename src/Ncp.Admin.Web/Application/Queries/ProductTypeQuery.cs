using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 产品类型单条 DTO（用于编辑回填、下拉）
/// </summary>
public record ProductTypeDto(ProductTypeId Id, string Name, int SortOrder, bool Visible);

/// <summary>
/// 产品类型查询
/// </summary>
public class ProductTypeQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 获取单条（用于编辑）
    /// </summary>
    public async Task<ProductTypeDto?> GetByIdAsync(ProductTypeId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductTypes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductTypeDto(x.Id, x.Name, x.SortOrder, x.Visible))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 获取列表（用于下拉、产品表单；可选仅可见）
    /// </summary>
    public async Task<IEnumerable<ProductTypeDto>> GetListAsync(bool includeInvisible = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ProductTypes.AsNoTracking();
        if (!includeInvisible)
            query = query.Where(x => x.Visible);
        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ProductTypeDto(x.Id, x.Name, x.SortOrder, x.Visible))
            .ToListAsync(cancellationToken);
    }
}
