using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 请假余额查询DTO
/// </summary>
public record LeaveBalanceQueryDto(
    LeaveBalanceId Id,
    UserId UserId,
    int Year,
    LeaveType LeaveType,
    decimal TotalDays,
    decimal UsedDays,
    decimal RemainingDays,
    DateTimeOffset CreatedAt);

/// <summary>
/// 请假余额查询
/// </summary>
public class LeaveBalanceQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<LeaveBalanceQueryDto?> GetByUserYearTypeAsync(UserId userId, int year, LeaveType leaveType, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.LeaveBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Year == year && b.LeaveType == leaveType, cancellationToken);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<List<LeaveBalanceQueryDto>> GetByUserYearAsync(UserId userId, int year, CancellationToken cancellationToken = default)
    {
        var list = await dbContext.LeaveBalances
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.Year == year)
            .OrderBy(b => b.LeaveType)
            .ToListAsync(cancellationToken);
        return list.Select(ToDto).ToList();
    }

    public async Task<PagedData<LeaveBalanceQueryDto>> GetPagedAsync(LeaveBalanceQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.LeaveBalances.AsNoTracking();
        if (input.UserId != null)
            query = query.Where(b => b.UserId == input.UserId);
        if (input.Year.HasValue)
            query = query.Where(b => b.Year == input.Year.Value);
        if (input.LeaveType.HasValue)
            query = query.Where(b => b.LeaveType == input.LeaveType.Value);

        return await query
            .OrderBy(b => b.Year)
            .ThenBy(b => b.UserId)
            .ThenBy(b => b.LeaveType)
            .Select(b => new LeaveBalanceQueryDto(
                b.Id,
                b.UserId,
                b.Year,
                b.LeaveType,
                b.TotalDays,
                b.UsedDays,
                b.TotalDays - b.UsedDays,
                b.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }

    private static LeaveBalanceQueryDto ToDto(LeaveBalance b) =>
        new(b.Id, b.UserId, b.Year, b.LeaveType, b.TotalDays, b.UsedDays, b.RemainingDays, b.CreatedAt);
}

/// <summary>
/// 请假余额查询入参
/// </summary>
public class LeaveBalanceQueryInput : PageRequest
{
    public UserId? UserId { get; set; }
    public int? Year { get; set; }
    public LeaveType? LeaveType { get; set; }
}
