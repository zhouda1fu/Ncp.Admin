namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 将通知发送推迟到 MediatR 管道中 UnitOfWork 提交之后，由 <see cref="NotificationPushAfterUnitOfWorkBehavior{TRequest,TResponse}"/> 统一投递。
/// </summary>
public sealed class DeferredNotificationSender(INotificationPushBuffer buffer) : INotificationSender
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        buffer.Enqueue(message);
        return Task.CompletedTask;
    }
}
