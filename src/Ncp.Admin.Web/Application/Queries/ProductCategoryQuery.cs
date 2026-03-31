using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 产品分类单条 DTO（用于编辑回填）
/// </summary>
public record ProductCategoryDto(
    ProductCategoryId Id,
    string Name,
    string Remark,
    ProductCategoryId? ParentId,
    int SortOrder,
    bool Visible,
    bool IsDiscount);

/// <summary>
/// 产品分类树节点 DTO
/// </summary>
public record ProductCategoryTreeDto(
    ProductCategoryId Id,
    string Name,
    string Remark,
    ProductCategoryId? ParentId,
    int SortOrder,
    bool Visible,
    bool IsDiscount,
    IEnumerable<ProductCategoryTreeDto> Children);

/// <summary>
/// 产品分类查询
/// </summary>
public class ProductCategoryQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 获取单条分类（用于编辑）
    /// </summary>
    public async Task<ProductCategoryDto?> GetByIdAsync(ProductCategoryId id, CancellationToken cancellationToken = default)
    {
        var c = await dbContext.ProductCategories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductCategoryDto(
                x.Id,
                x.Name,
                x.Remark,
                x.ParentId == new ProductCategoryId(Guid.Empty) ? null : x.ParentId,
                x.SortOrder,
                x.Visible,
                x.IsDiscount))
            .FirstOrDefaultAsync(cancellationToken);
        return c;
    }

    /// <summary>
    /// 获取产品分类树（仅可见）
    /// </summary>
    public async Task<IEnumerable<ProductCategoryTreeDto>> GetTreeAsync(bool includeInvisible = false, CancellationToken cancellationToken = default)
    {
        var list = await dbContext.ProductCategories
            .AsNoTracking()
            .Where(c => includeInvisible || c.Visible)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ProductCategoryNode(c.Id, c.Name, c.Remark, c.ParentId, c.SortOrder, c.Visible, c.IsDiscount))
            .ToListAsync(cancellationToken);

        var dict = list.ToDictionary(x => x.Id);
        var emptyId = new ProductCategoryId(Guid.Empty);
        // 投影时 EF 可能未对 ParentId 应用 ValueConverter，DB 的 NULL 在 list 中为 null，需同时视为根节点
        var roots = list.Where(x => x.ParentId == null || x.ParentId == emptyId).OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList();
        return roots.Select(r => BuildNode(r, dict));
    }

    private static ProductCategoryTreeDto BuildNode(
        ProductCategoryNode r,
        Dictionary<ProductCategoryId, ProductCategoryNode> dict)
    {
        var children = dict.Values
            .Where(x => x.ParentId == r.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(c => BuildNode(c, dict))
            .ToList();
        var emptyId = new ProductCategoryId(Guid.Empty);
        return new ProductCategoryTreeDto(
            r.Id,
            r.Name,
            r.Remark,
            (r.ParentId == null || r.ParentId == emptyId) ? null : r.ParentId,
            r.SortOrder,
            r.Visible,
            r.IsDiscount,
            children);
    }

    /// <summary>
    /// 投影时 EF 可能不对 ParentId 应用 ValueConverter，DB 的 NULL 会以 null 出现，故用可空类型。
    /// </summary>
    private sealed record ProductCategoryNode(
        ProductCategoryId Id,
        string Name,
        string Remark,
        ProductCategoryId? ParentId,
        int SortOrder,
        bool Visible,
        bool IsDiscount);
}
