using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Notification;

namespace Ncp.Admin.Web.Application.Commands.Notification;

/// <summary>
/// 发送通知命令
/// </summary>
public record SendNotificationCommand(
    string Title,
    string Content,
    NotificationType Type,
    NotificationLevel Level,
    long? SenderId,
    string SenderName,
    long ReceiverId,
    string? BusinessId = null,
    string? BusinessType = null) : ICommand<NotificationId>;

/// <summary>
/// 发送通知命令处理器
/// </summary>
public class SendNotificationCommandHandler(
    INotificationRepository notificationRepository,
    INotificationSender notificationSender) : ICommandHandler<SendNotificationCommand, NotificationId>
{
    public async Task<NotificationId> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Domain.AggregatesModel.NotificationAggregate.Notification(
            request.Title,
            request.Content,
            request.Type,
            request.Level,
            request.SenderId,
            request.SenderName,
            request.ReceiverId,
            request.BusinessId,
            request.BusinessType);

        await notificationRepository.AddAsync(notification, cancellationToken);

        var message = new NotificationMessage(
            request.ReceiverId,
            notification.Id,
            request.Title,
            request.Content,
            request.Type,
            request.Level,
            request.SenderName,
            request.BusinessId,
            request.BusinessType,
            notification.CreatedAt);
        await notificationSender.SendAsync(message, cancellationToken);

        return notification.Id;
    }
}
