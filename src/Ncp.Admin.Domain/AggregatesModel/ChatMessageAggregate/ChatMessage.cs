using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate;

/// <summary>
/// 聊天消息ID（强类型ID）
/// </summary>
public partial record ChatMessageId : IGuidStronglyTypedId;

/// <summary>
/// 聊天消息聚合根
/// </summary>
public class ChatMessage : Entity<ChatMessageId>, IAggregateRoot
{
    protected ChatMessage() { }

    /// <summary>
    /// 所属聊天组ID
    /// </summary>
    public ChatGroupId ChatGroupId { get; private set; } = default!;

    /// <summary>
    /// 发送人用户ID
    /// </summary>
    public UserId SenderId { get; private set; } = default!;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// 回复的消息ID（可选，用于 @ 或引用回复）
    /// </summary>
    public ChatMessageId? ReplyToMessageId { get; private set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 创建一条消息
    /// </summary>
    public ChatMessage(ChatGroupId chatGroupId, UserId senderId, string content, ChatMessageId? replyToMessageId = null)
    {
        ChatGroupId = chatGroupId;
        SenderId = senderId;
        Content = content ;
        ReplyToMessageId = replyToMessageId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
