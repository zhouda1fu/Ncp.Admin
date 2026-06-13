namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 在命令工作单元提交前暂存待推送通知，提交后再统一发送（避免 SignalR 触发时库中尚无记录）。
/// </summary>
public interface INotificationPushBuffer
{
    void Enqueue(NotificationMessage message);

    IReadOnlyList<NotificationMessage> TakeAll();
}

/// <inheritdoc />
public sealed class NotificationPushBuffer : INotificationPushBuffer
{
    private readonly List<NotificationMessage> _pending = [];

    public void Enqueue(NotificationMessage message) => _pending.Add(message);

    public IReadOnlyList<NotificationMessage> TakeAll()
    {
        if (_pending.Count == 0)
        {
            return [];
        }

        var copy = _pending.ToList();
        _pending.Clear();
        return copy;
    }
}
