namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 组合通知发送器：单个通道失败不阻断其它通道。
/// </summary>
public class CompositeNotificationSender(
    IEnumerable<INotificationChannel> channels,
    ILogger<CompositeNotificationSender> logger) : INotificationSender
{
    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        foreach (var channel in channels)
        {
            try
            {
                await channel.SendAsync(message, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "通知通道发送失败，通道：{ChannelName}，接收人ID：{ReceiverId}，通知ID：{NotificationId}",
                    channel.GetType().Name,
                    message.ReceiverId,
                    message.NotificationId?.Id);
            }
        }
    }
}
