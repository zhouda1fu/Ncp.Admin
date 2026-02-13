using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Web.Application.Commands.Notification;

/// <summary>
/// 删除通知命令
/// </summary>
public record DeleteNotificationCommand(NotificationId Id) : ICommand;

/// <summary>
/// 删除通知命令处理器
/// </summary>
public class DeleteNotificationCommandHandler(INotificationRepository notificationRepository) : ICommandHandler<DeleteNotificationCommand>
{
    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException($"未找到通知，Id = {request.Id}", ErrorCodes.NotificationNotFound);

        notification.SoftDelete();
    }
}
