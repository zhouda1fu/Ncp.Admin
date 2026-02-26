using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record VehicleBookingQueryDto(
    VehicleBookingId Id,
    VehicleId VehicleId,
    string PlateNumber,
    string Model,
    UserId BookerId,
    string Purpose,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    VehicleBookingStatus Status,
    DateTimeOffset CreatedAt);

public class VehicleBookingQueryInput : PageRequest
{
    public Guid? VehicleId { get; set; }
    public long? BookerId { get; set; }
    public VehicleBookingStatus? Status { get; set; }
}

public class VehicleBookingQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<VehicleBookingQueryDto?> GetByIdAsync(VehicleBookingId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.VehicleBookings
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Join(dbContext.Vehicles, bk => bk.VehicleId, v => v.Id, (bk, v) => new { bk, v })
            .Select(x => new VehicleBookingQueryDto(x.bk.Id, x.bk.VehicleId, x.v.PlateNumber, x.v.Model, x.bk.BookerId, x.bk.Purpose, x.bk.StartAt, x.bk.EndAt, x.bk.Status, x.bk.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<VehicleBookingQueryDto>> GetPagedAsync(VehicleBookingQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.VehicleBookings
            .AsNoTracking()
            .Join(dbContext.Vehicles, bk => bk.VehicleId, v => v.Id, (bk, v) => new { bk, v });
        if (input.VehicleId.HasValue)
            query = query.Where(x => x.bk.VehicleId == new VehicleId(input.VehicleId.Value));
        if (input.BookerId.HasValue)
            query = query.Where(x => x.bk.BookerId == new UserId(input.BookerId.Value));
        if (input.Status.HasValue)
            query = query.Where(x => x.bk.Status == input.Status.Value);
        return await query
            .OrderByDescending(x => x.bk.StartAt)
            .Select(x => new VehicleBookingQueryDto(x.bk.Id, x.bk.VehicleId, x.v.PlateNumber, x.v.Model, x.bk.BookerId, x.bk.Purpose, x.bk.StartAt, x.bk.EndAt, x.bk.Status, x.bk.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
