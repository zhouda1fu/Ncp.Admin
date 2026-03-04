using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 产品查询 DTO
/// </summary>
public record ProductQueryDto(
    ProductId Id,
    string Name,
    string Code,
    string Model,
    string Unit);

public class ProductQueryInput : PageRequest
{
    public string? Keyword { get; set; }
}

public class ProductQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<ProductQueryDto?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductQueryDto(p.Id, p.Name, p.Code, p.Model, p.Unit))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<ProductQueryDto>> GetPagedAsync(ProductQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var k = input.Keyword.Trim();
            query = query.Where(p => p.Name.Contains(k) || p.Code.Contains(k) || p.Model.Contains(k));
        }
        return await query
            .OrderBy(p => p.Code)
            .Select(p => new ProductQueryDto(p.Id, p.Name, p.Code, p.Model, p.Unit))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
