using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Notifications;

/// <summary>
/// 批量标记通知为已读命令
/// </summary>
public record MarkAllNotificationsReadCommand(long ReceiverId) : ICommand<int>;

/// <summary>
/// 批量标记通知为已读命令处理器
/// </summary>
public class MarkAllNotificationsReadCommandHandler(INotificationRepository notificationRepository) : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    public async Task<int> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        return await notificationRepository.MarkAllReadAsync(request.ReceiverId, DateTimeOffset.UtcNow, cancellationToken);
    }
}
