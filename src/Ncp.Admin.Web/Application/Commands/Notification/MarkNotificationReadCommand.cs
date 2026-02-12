using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Web.Application.Commands.Notification;

/// <summary>
/// 标记通知为已读命令
/// </summary>
public record MarkNotificationReadCommand(NotificationId Id) : ICommand;

/// <summary>
/// 标记通知为已读命令处理器
/// </summary>
public class MarkNotificationReadCommandHandler(INotificationRepository notificationRepository) : ICommandHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException($"未找到通知，Id = {request.Id}", ErrorCodes.NotificationNotFound);

        notification.MarkAsRead();
    }
}
