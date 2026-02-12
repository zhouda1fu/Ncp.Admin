namespace Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;

/// <summary>
/// 通知ID（强类型ID）
/// </summary>
public partial record NotificationId : IInt64StronglyTypedId;

/// <summary>
/// 通知聚合根
/// 用于管理站内通知消息
/// </summary>
public class Notification : Entity<NotificationId>, IAggregateRoot
{
    protected Notification()
    {
    }

    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// 通知类型（System=系统通知, Workflow=审批通知, Announcement=公告通知）
    /// </summary>
    public NotificationType Type { get; private set; }

    /// <summary>
    /// 通知级别（Info, Warning, Error, Success）
    /// </summary>
    public NotificationLevel Level { get; private set; } = NotificationLevel.Info;

    /// <summary>
    /// 发送人ID（系统通知时为空）
    /// </summary>
    public long? SenderId { get; private set; }

    /// <summary>
    /// 发送人名称
    /// </summary>
    public string SenderName { get; private set; } = string.Empty;

    /// <summary>
    /// 接收人ID
    /// </summary>
    public long ReceiverId { get; private set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>
    /// 关联业务ID（如工作流实例ID）
    /// </summary>
    public string? BusinessId { get; private set; }

    /// <summary>
    /// 关联业务类型
    /// </summary>
    public string? BusinessType { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 删除时间
    /// </summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建通知
    /// </summary>
    public Notification(string title, string content, NotificationType type, NotificationLevel level,
        long? senderId, string senderName, long receiverId, string? businessId = null, string? businessType = null)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Title = title;
        Content = content;
        Type = type;
        Level = level;
        SenderId = senderId;
        SenderName = senderName;
        ReceiverId = receiverId;
        BusinessId = businessId;
        BusinessType = businessType;
        IsRead = false;
    }

    /// <summary>
    /// 标记为已读
    /// </summary>
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// 软删除
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted)
        {
            throw new KnownException("通知已经被删除", ErrorCodes.NotificationAlreadyDeleted);
        }

        IsDeleted = true;
    }
}

/// <summary>
/// 通知类型
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 系统通知
    /// </summary>
    System = 0,

    /// <summary>
    /// 审批通知
    /// </summary>
    Workflow = 1,

    /// <summary>
    /// 公告通知
    /// </summary>
    Announcement = 2
}

/// <summary>
/// 通知级别
/// </summary>
public enum NotificationLevel
{
    /// <summary>
    /// 信息
    /// </summary>
    Info = 0,

    /// <summary>
    /// 成功
    /// </summary>
    Success = 1,

    /// <summary>
    /// 警告
    /// </summary>
    Warning = 2,

    /// <summary>
    /// 错误
    /// </summary>
    Error = 3
}
