using Microsoft.AspNetCore.SignalR;
using Ncp.Admin.Web.Application.Hubs;

namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 通过 SignalR 向指定用户连接推送站内通知
/// </summary>
public class SignalRNotificationSender(IHubContext<NotificationHub> hubContext) : INotificationSender
{
    public const string MethodName = "ReceiveNotification";

    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            message.NotificationId,
            message.Title,
            message.Content,
            message.Type,
            message.Level,
            message.SenderName,
            message.BusinessId,
            message.BusinessType,
            message.CreatedAt,
        };
        await hubContext.Clients
            .User(message.ReceiverId.ToString())
            .SendAsync(MethodName, payload, cancellationToken);
    }
}
