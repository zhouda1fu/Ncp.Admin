using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 产品查询 DTO
/// </summary>
public record ProductQueryDto(
    ProductId Id,
    ProductTypeId ProductTypeId,
    bool Status,
    string Name,
    string Code,
    string Model,
    string Unit,
    string Barcode,
    string ActivationCode,
    string PriceStandard,
    string MarketSales,
    string Description,
    decimal CostPrice,
    decimal CustomerPrice,
    int Qty,
    string Tags,
    string Feature,
    string Configuration,
    string Instructions,
    string InstallProcess,
    string OperationProcessResources,
    string Introduction,
    string IntroductionResources,
    string ImagePath,
    ProductCategoryId? CategoryId,
    SupplierId? SupplierId,
    string CategoryName);

public class ProductQueryInput : PageRequest
{
    public string? Keyword { get; set; }
}

public class ProductQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<ProductQueryDto?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var emptyCategoryId = new ProductCategoryId(Guid.Empty);
        var emptySupplierId = new SupplierId(Guid.Empty);
        return await (
            from p in dbContext.Products.AsNoTracking()
            where p.Id == id
            join c in dbContext.ProductCategories.AsNoTracking() on p.CategoryId equals c.Id into catJoin
            from c in catJoin.DefaultIfEmpty()
            select new ProductQueryDto(
                p.Id, p.ProductTypeId, p.Status, p.Name, p.Code, p.Model, p.Unit, p.Barcode, p.ActivationCode, p.PriceStandard, p.MarketSales,
                p.Description, p.CostPrice, p.CustomerPrice, p.Qty, p.Tags, p.Feature, p.Configuration, p.Instructions,
                p.InstallProcess, p.OperationProcessResources, p.Introduction, p.IntroductionResources, p.ImagePath,
                p.CategoryId == emptyCategoryId ? null : p.CategoryId,
                p.SupplierId == emptySupplierId ? null : p.SupplierId,
                c != null ? c.Name : string.Empty))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<ProductQueryDto>> GetPagedAsync(ProductQueryInput input, CancellationToken cancellationToken = default)
    {
        var emptyCategoryId = new ProductCategoryId(Guid.Empty);
        var emptySupplierId = new SupplierId(Guid.Empty);
        var query = dbContext.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var k = input.Keyword.Trim();
            query = query.Where(p => p.Name.Contains(k) || p.Code.Contains(k) || p.Model.Contains(k));
        }

        var joinedQuery =
            from p in query
            join c in dbContext.ProductCategories.AsNoTracking() on p.CategoryId equals c.Id into catJoin
            from c in catJoin.DefaultIfEmpty()
            orderby p.Code
            select new ProductQueryDto(
                p.Id, p.ProductTypeId, p.Status, p.Name, p.Code, p.Model, p.Unit, p.Barcode, p.ActivationCode, p.PriceStandard, p.MarketSales,
                p.Description, p.CostPrice, p.CustomerPrice, p.Qty, p.Tags, p.Feature, p.Configuration, p.Instructions,
                p.InstallProcess, p.OperationProcessResources, p.Introduction, p.IntroductionResources, p.ImagePath,
                p.CategoryId == emptyCategoryId ? null : p.CategoryId,
                p.SupplierId == emptySupplierId ? null : p.SupplierId,
                c != null ? c.Name : string.Empty);

        return await joinedQuery.ToPagedDataAsync(input, cancellationToken);
    }
}
