using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Services.Notification;

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
/// 通知列表查询 DTO（含按接收人角色解析的跳转路径）。
/// </summary>
/// <param name="Id">通知 ID。</param>
/// <param name="Title">标题。</param>
/// <param name="Content">内容摘要（列表接口截断）。</param>
/// <param name="Type">类型。</param>
/// <param name="Level">级别。</param>
/// <param name="SenderName">发送人名称。</param>
/// <param name="IsRead">是否已读。</param>
/// <param name="ReadAt">已读时间。</param>
/// <param name="BusinessId">业务关联 ID。</param>
/// <param name="BusinessType">业务类型。</param>
/// <param name="LinkPath">站内跳转 path。</param>
/// <param name="LinkQuery">跳转 query 参数（可为空）。</param>
/// <param name="CreatedAt">创建时间。</param>
public record NotificationListQueryDto(
    NotificationId Id,
    string Title,
    string Content,
    NotificationType Type,
    NotificationLevel Level,
    string SenderName,
    bool IsRead,
    DateTimeOffset? ReadAt,
    string? BusinessId,
    string? BusinessType,
    string? LinkPath,
    IReadOnlyDictionary<string, string>? LinkQuery,
    DateTimeOffset CreatedAt);

/// <summary>
/// 通知列表查询入参
/// </summary>
public class NotificationQueryInput : PageRequest
{
    public NotificationType? Type { get; set; }

    public bool? IsRead { get; set; }

    /// <summary>
    /// 是否在响应中附带未读总数（铃铛场景需要；仅校验列表时可设为 false 以减少一次 COUNT）。
    /// </summary>
    public bool IncludeUnreadCount { get; set; } = true;
}

/// <summary>
/// 通知列表查询结果
/// </summary>
public record NotificationListQueryResult(PagedData<NotificationListQueryDto> Page, int UnreadCount);

/// <summary>
/// 通知查询服务
/// </summary>
public class NotificationQuery(
    ApplicationDbContext applicationDbContext,
    NotificationNavigationResolver notificationNavigationResolver) : IQuery
{
    private const int ListContentPreviewMaxLength = 300;

    private DbSet<Notification> NotificationSet { get; } = applicationDbContext.Notifications;

    /// <summary>
    /// 获取用户的通知列表（分页，含跳转路径解析）。
    /// </summary>
    public async Task<NotificationListQueryResult> GetNotificationListAsync(
        UserId userId,
        NotificationQueryInput query,
        CancellationToken cancellationToken)
    {
        var queryable = NotificationSet.AsNoTracking()
            .Where(n => n.ReceiverId == userId.Id)
            .WhereIf(query.Type.HasValue, n => n.Type == query.Type)
            .WhereIf(query.IsRead.HasValue, n => n.IsRead == query.IsRead);

        var paged = await queryable
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationQueryDto(
                n.Id, n.Title, n.Content, n.Type, n.Level,
                n.SenderId, n.SenderName, n.ReceiverId,
                n.IsRead, n.ReadAt,
                n.BusinessId, n.BusinessType, n.CreatedAt))
            .ToPagedDataAsync(query, cancellationToken);

        var navigationByBusinessKey = await notificationNavigationResolver.ResolveBatchAsync(
            userId,
            paged.Items.Select(n => (n.BusinessType, n.BusinessId, n.SenderId)),
            cancellationToken);

        var items = paged.Items.Select(n =>
        {
            navigationByBusinessKey.TryGetValue(
                NotificationNavigationResolver.ToCacheKey(n.BusinessType, n.BusinessId),
                out var navigation);
            return new NotificationListQueryDto(
                n.Id,
                n.Title,
                TruncateContentPreview(n.Content),
                n.Type,
                n.Level,
                NotificationSenderDisplayName.Resolve(n.SenderName),
                n.IsRead,
                n.ReadAt,
                n.BusinessId,
                n.BusinessType,
                navigation?.Path,
                navigation?.Query,
                n.CreatedAt);
        }).ToList();

        var unreadCount = query.IncludeUnreadCount
            ? await GetUnreadCountAsync(userId, cancellationToken)
            : 0;

        return new NotificationListQueryResult(
            new PagedData<NotificationListQueryDto>(items, paged.Total, paged.PageIndex, paged.PageSize),
            unreadCount);
    }

    /// <summary>
    /// 获取用户未读通知数量
    /// </summary>
    public Task<int> GetUnreadCountAsync(long receiverId, CancellationToken cancellationToken) =>
        GetUnreadCountAsync(new UserId(receiverId), cancellationToken);

    /// <summary>
    /// 获取用户未读通知数量。
    /// </summary>
    public Task<int> GetUnreadCountAsync(UserId userId, CancellationToken cancellationToken) =>
        NotificationSet.AsNoTracking()
            .Where(n => n.ReceiverId == userId.Id && !n.IsRead)
            .CountAsync(cancellationToken);

    /// <summary>
    /// 根据ID获取通知
    /// </summary>
    public async Task<NotificationQueryDto?> GetNotificationByIdAsync(NotificationId id, CancellationToken cancellationToken = default)
    {
        var notification = await NotificationSet.AsNoTracking()
            .Where(n => n.Id == id)
            .Select(n => new NotificationQueryDto(
                n.Id, n.Title, n.Content, n.Type, n.Level,
                n.SenderId, n.SenderName, n.ReceiverId,
                n.IsRead, n.ReadAt,
                n.BusinessId, n.BusinessType, n.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (notification is null)
        {
            return null;
        }

        return notification with
        {
            SenderName = NotificationSenderDisplayName.Resolve(notification.SenderName),
        };
    }

    private static string TruncateContentPreview(string? content)
    {
        var text = (content ?? string.Empty).Trim();
        if (text.Length <= ListContentPreviewMaxLength)
        {
            return text;
        }

        return $"{text[..ListContentPreviewMaxLength]}...";
    }
}
