using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record AssetAllocationQueryDto(
    AssetAllocationId Id,
    AssetId AssetId,
    string AssetName,
    string AssetCode,
    UserId UserId,
    DateTimeOffset AllocatedAt,
    DateTimeOffset? ReturnedAt,
    string? Note);

public class AssetAllocationQueryInput : PageRequest
{
    public Guid? AssetId { get; set; }
    public long? UserId { get; set; }
    public bool? Returned { get; set; }
}

public class AssetAllocationQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<AssetAllocationQueryDto?> GetByIdAsync(AssetAllocationId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AssetAllocations
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Join(dbContext.Assets, al => al.AssetId, asst => asst.Id, (al, asst) => new { al, asst })
            .Select(x => new AssetAllocationQueryDto(x.al.Id, x.al.AssetId, x.asst.Name, x.asst.Code, x.al.UserId, x.al.AllocatedAt, x.al.ReturnedAt, x.al.Note))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<AssetAllocationQueryDto>> GetPagedAsync(AssetAllocationQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AssetAllocations
            .AsNoTracking()
            .Join(dbContext.Assets, al => al.AssetId, asst => asst.Id, (al, asst) => new { al, asst });
        if (input.AssetId.HasValue)
            query = query.Where(x => x.al.AssetId == new AssetId(input.AssetId.Value));
        if (input.UserId.HasValue)
            query = query.Where(x => x.al.UserId == new UserId(input.UserId.Value));
        if (input.Returned.HasValue)
            query = query.Where(x => input.Returned.Value ? x.al.ReturnedAt != null : x.al.ReturnedAt == null);
        return await query
            .OrderByDescending(x => x.al.AllocatedAt)
            .Select(x => new AssetAllocationQueryDto(x.al.Id, x.al.AssetId, x.asst.Name, x.asst.Code, x.al.UserId, x.al.AllocatedAt, x.al.ReturnedAt, x.al.Note))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
