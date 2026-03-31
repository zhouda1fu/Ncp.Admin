using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 产品参数查询 DTO
/// </summary>
public record ProductParameterDto(ProductParameterId Id, ProductId ProductId, string Year, string Description);

/// <summary>
/// 产品参数查询
/// </summary>
public class ProductParameterQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按产品 ID 获取参数列表
    /// </summary>
    public async Task<IReadOnlyList<ProductParameterDto>> GetByProductIdAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductParameters
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .OrderBy(p => p.Year)
            .Select(p => new ProductParameterDto(p.Id, p.ProductId, p.Year, p.Description))
            .ToListAsync(cancellationToken);
    }
}
