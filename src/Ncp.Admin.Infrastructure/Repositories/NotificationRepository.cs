using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 通知仓储接口
/// </summary>
public interface INotificationRepository : IRepository<Notification, NotificationId> { }

/// <summary>
/// 通知仓储实现
/// </summary>
public class NotificationRepository(ApplicationDbContext context) : RepositoryBase<Notification, NotificationId, ApplicationDbContext>(context), INotificationRepository { }
