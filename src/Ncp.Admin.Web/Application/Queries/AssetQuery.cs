using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record AssetQueryDto(
    AssetId Id,
    string Name,
    string Category,
    string Code,
    AssetStatus Status,
    DateTimeOffset PurchaseDate,
    decimal Value,
    string? Remark,
    UserId CreatorId,
    DateTimeOffset CreatedAt);

public class AssetQueryInput : PageRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public AssetStatus? Status { get; set; }
}

public class AssetQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<AssetQueryDto?> GetByIdAsync(AssetId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Assets
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new AssetQueryDto(c.Id, c.Name, c.Category, c.Code, c.Status, c.PurchaseDate, c.Value, c.Remark, c.CreatorId, c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<AssetQueryDto>> GetPagedAsync(AssetQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Assets.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Code))
            query = query.Where(c => c.Code.Contains(input.Code));
        if (!string.IsNullOrWhiteSpace(input.Name))
            query = query.Where(c => c.Name.Contains(input.Name));
        if (input.Status.HasValue)
            query = query.Where(c => c.Status == input.Status.Value);
        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new AssetQueryDto(c.Id, c.Name, c.Category, c.Code, c.Status, c.PurchaseDate, c.Value, c.Remark, c.CreatorId, c.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
