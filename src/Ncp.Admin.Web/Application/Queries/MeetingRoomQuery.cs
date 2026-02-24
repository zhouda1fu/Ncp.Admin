using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 会议室查询 DTO
/// </summary>
public record MeetingRoomQueryDto(MeetingRoomId Id, string Name, string? Location, int Capacity, string? Equipment, int Status, DateTimeOffset CreatedAt);

/// <summary>
/// 会议室分页查询入参
/// </summary>
public class MeetingRoomQueryInput : PageRequest
{
    /// <summary>
    /// 名称关键字
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 状态筛选（0 禁用 1 可用）
    /// </summary>
    public int? Status { get; set; }
}

/// <summary>
/// 会议室查询服务
/// </summary>
public class MeetingRoomQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询会议室
    /// </summary>
    public async Task<MeetingRoomQueryDto?> GetByIdAsync(MeetingRoomId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.MeetingRooms
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new MeetingRoomQueryDto(r.Id, r.Name, r.Location, r.Capacity, r.Equipment, r.Status, r.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询会议室
    /// </summary>
    public async Task<PagedData<MeetingRoomQueryDto>> GetPagedAsync(MeetingRoomQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.MeetingRooms.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Name))
            query = query.Where(r => r.Name.Contains(input.Name));
        if (input.Status.HasValue)
            query = query.Where(r => r.Status == input.Status.Value);
        return await query
            .OrderBy(r => r.Name)
            .Select(r => new MeetingRoomQueryDto(r.Id, r.Name, r.Location, r.Capacity, r.Equipment, r.Status, r.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
