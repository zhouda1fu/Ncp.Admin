using System;
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
    /// <summary>非回复消息的占位引用（<see cref="Guid.Empty"/>）</summary>
    public static ChatMessageId NoReplyToMessageId { get; } = new(Guid.Empty);

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
    /// 回复的消息 ID（非回复时为 <see cref="NoReplyToMessageId"/>）
    /// </summary>
    public ChatMessageId ReplyToMessageId { get; private set; } = NoReplyToMessageId;

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
        ReplyToMessageId = replyToMessageId ?? NoReplyToMessageId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
