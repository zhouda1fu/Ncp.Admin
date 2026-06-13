namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 在命令处理与 UnitOfWork 保存完成后，再向 SignalR / 微信等通道推送通知。
/// </summary>
public sealed class NotificationPushAfterUnitOfWorkBehavior<TRequest, TResponse>(
    INotificationPushBuffer buffer,
    CompositeNotificationSender notificationSender,
    ILogger<NotificationPushAfterUnitOfWorkBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        finally
        {
            var messages = buffer.TakeAll();
            foreach (var message in messages)
            {
                try
                {
                    await notificationSender.SendAsync(message, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "通知推送失败（已持久化），接收人ID：{ReceiverId}，通知ID：{NotificationId}",
                        message.ReceiverId,
                        message.NotificationId?.Id);
                }
            }
        }
    }
}
