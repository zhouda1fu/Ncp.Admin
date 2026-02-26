using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record ContractQueryDto(
    ContractId Id,
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    ContractStatus Status,
    string? FileStorageKey,
    UserId CreatorId,
    DateTimeOffset CreatedAt);

public class ContractQueryInput : PageRequest
{
    public string? Code { get; set; }
    public string? Title { get; set; }
    public ContractStatus? Status { get; set; }
}

public class ContractQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<ContractQueryDto?> GetByIdAsync(ContractId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Contracts
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ContractQueryDto(c.Id, c.Code, c.Title, c.PartyA, c.PartyB, c.Amount, c.StartDate, c.EndDate, c.Status, c.FileStorageKey, c.CreatorId, c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<ContractQueryDto>> GetPagedAsync(ContractQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Contracts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Code))
            query = query.Where(c => c.Code.Contains(input.Code));
        if (!string.IsNullOrWhiteSpace(input.Title))
            query = query.Where(c => c.Title.Contains(input.Title));
        if (input.Status.HasValue)
            query = query.Where(c => c.Status == input.Status.Value);
        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContractQueryDto(c.Id, c.Code, c.Title, c.PartyA, c.PartyB, c.Amount, c.StartDate, c.EndDate, c.Status, c.FileStorageKey, c.CreatorId, c.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
