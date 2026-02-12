using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;

namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 通知推送消息 DTO（站内/邮件/微信等通道通用）
/// </summary>
public record NotificationMessage(
    long ReceiverId,
    Ncp.Admin.Domain.AggregatesModel.NotificationAggregate.NotificationId? NotificationId,
    string Title,
    string Content,
    NotificationType Type,
    NotificationLevel Level,
    string? SenderName,
    string? BusinessId,
    string? BusinessType,
    DateTimeOffset CreatedAt);

/// <summary>
/// 通知发送抽象（站内 SignalR、邮件、微信等）
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// 发送通知到指定接收人（具体通道由实现决定）
    /// </summary>
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
