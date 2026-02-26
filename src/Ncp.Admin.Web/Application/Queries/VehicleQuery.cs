using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record VehicleQueryDto(
    VehicleId Id,
    string PlateNumber,
    string Model,
    VehicleStatus Status,
    string? Remark,
    DateTimeOffset CreatedAt);

public class VehicleQueryInput : PageRequest
{
    public string? PlateNumber { get; set; }
    public VehicleStatus? Status { get; set; }
}

public class VehicleQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<VehicleQueryDto?> GetByIdAsync(VehicleId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehicleQueryDto(v.Id, v.PlateNumber, v.Model, v.Status, v.Remark, v.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<VehicleQueryDto>> GetPagedAsync(VehicleQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Vehicles.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.PlateNumber))
            query = query.Where(v => v.PlateNumber.Contains(input.PlateNumber));
        if (input.Status.HasValue)
            query = query.Where(v => v.Status == input.Status.Value);
        return await query
            .OrderBy(v => v.PlateNumber)
            .Select(v => new VehicleQueryDto(v.Id, v.PlateNumber, v.Model, v.Status, v.Remark, v.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
