using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 通知仓储接口
/// </summary>
public interface INotificationRepository : IRepository<Notification, NotificationId>
{
    /// <summary>
    /// 将指定接收人的所有未读通知标记为已读。
    /// </summary>
    Task<int> MarkAllReadAsync(long receiverId, DateTimeOffset readAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定业务对象关联的通知。
    /// </summary>
    Task<int> RemoveByBusinessAsync(string businessType, string businessId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 通知仓储实现
/// </summary>
public class NotificationRepository(ApplicationDbContext context) : RepositoryBase<Notification, NotificationId, ApplicationDbContext>(context), INotificationRepository
{
    public Task<int> MarkAllReadAsync(long receiverId, DateTimeOffset readAt, CancellationToken cancellationToken = default)
    {
        return context.Notifications
            .Where(n => n.ReceiverId == receiverId && !n.IsRead && !n.IsDeleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, readAt),
            cancellationToken);
    }

    public async Task<int> RemoveByBusinessAsync(string businessType, string businessId, CancellationToken cancellationToken = default)
    {
        var notifications = await context.Notifications
            .Where(x => x.BusinessType == businessType && x.BusinessId == businessId)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return 0;
        }

        context.Notifications.RemoveRange(notifications);
        return notifications.Count;
    }
}
