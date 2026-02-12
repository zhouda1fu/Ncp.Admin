using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 通知查询DTO
/// </summary>
public record NotificationQueryDto(
    NotificationId Id,
    string Title,
    string Content,
    NotificationType Type,
    NotificationLevel Level,
    long? SenderId,
    string SenderName,
    long ReceiverId,
    bool IsRead,
    DateTimeOffset? ReadAt,
    string? BusinessId,
    string? BusinessType,
    DateTimeOffset CreatedAt);

/// <summary>
/// 通知查询输入参数
/// </summary>
public class NotificationQueryInput
{
    public NotificationType? Type { get; set; }
    public bool? IsRead { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 通知查询服务
/// </summary>
public class NotificationQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<Notification> NotificationSet { get; } = applicationDbContext.Notifications;

    /// <summary>
    /// 获取用户的通知列表（分页）
    /// </summary>
    public async Task<(IEnumerable<NotificationQueryDto> Items, int Total)> GetNotificationListAsync(
        long receiverId, NotificationQueryInput query, CancellationToken cancellationToken)
    {
        var queryable = NotificationSet.AsNoTracking()
            .Where(n => n.ReceiverId == receiverId)
            .WhereIf(query.Type.HasValue, n => n.Type == query.Type)
            .WhereIf(query.IsRead.HasValue, n => n.IsRead == query.IsRead);

        var total = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(n => n.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationQueryDto(
                n.Id, n.Title, n.Content, n.Type, n.Level,
                n.SenderId, n.SenderName, n.ReceiverId,
                n.IsRead, n.ReadAt,
                n.BusinessId, n.BusinessType, n.CreatedAt))
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <summary>
    /// 获取用户未读通知数量
    /// </summary>
    public async Task<int> GetUnreadCountAsync(long receiverId, CancellationToken cancellationToken)
    {
        return await NotificationSet.AsNoTracking()
            .Where(n => n.ReceiverId == receiverId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// 根据ID获取通知
    /// </summary>
    public async Task<NotificationQueryDto?> GetNotificationByIdAsync(NotificationId id, CancellationToken cancellationToken = default)
    {
        return await NotificationSet.AsNoTracking()
            .Where(n => n.Id == id)
            .Select(n => new NotificationQueryDto(
                n.Id, n.Title, n.Content, n.Type, n.Level,
                n.SenderId, n.SenderName, n.ReceiverId,
                n.IsRead, n.ReadAt,
                n.BusinessId, n.BusinessType, n.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
