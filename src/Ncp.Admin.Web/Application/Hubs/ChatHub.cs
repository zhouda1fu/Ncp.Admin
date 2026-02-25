using MediatR;
using Microsoft.AspNetCore.SignalR;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ChatMessage;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Hubs;

/// <summary>
/// 推送给客户端的消息 DTO（SignalR 序列化用）
/// </summary>
public class ChatMessagePushDto
{
    public string Id { get; set; } = "";
    public string ChatGroupId { get; set; } = "";
    public long SenderId { get; set; }
    public string SenderName { get; set; } = "";
    public string Content { get; set; } = "";
    public string? ReplyToMessageId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// 聊天客户端接口
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// 收到新消息（结构化）
    /// </summary>
    Task ReceiveMessageDto(ChatMessagePushDto message);

    /// <summary>
    /// 收到错误（如未登录、参数无效、发送失败等）
    /// </summary>
    Task ReceiveError(string message);
}

/// <summary>
/// 聊天 Hub：单聊/群聊、消息持久化、实时推送
/// </summary>
public class ChatHub(IMediator mediator, ChatMessageQuery chatMessageQuery) : Hub<IChatClient>
{
    public const string GroupPrefix = "ChatGroup_";

    /// <summary>
    /// 加入聊天组（连接后调用以接收该组消息）
    /// </summary>
    public async Task JoinGroup(string chatGroupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupPrefix + chatGroupId);
    }

    /// <summary>
    /// 离开聊天组
    /// </summary>
    public async Task LeaveGroup(string chatGroupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupPrefix + chatGroupId);
    }

    /// <summary>
    /// 发送消息：持久化并推送给组内所有人
    /// </summary>
    public async Task SendMessage(string chatGroupId, string content, string? replyToMessageId = null)
    {
        var userIdStr = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Clients.Caller.ReceiveError("未登录");
            return;
        }
        if (!Guid.TryParse(chatGroupId, out var groupIdGuid))
        {
            await Clients.Caller.ReceiveError("无效的聊天组");
            return;
        }
        ChatMessageId? replyId = null;
        if (!string.IsNullOrEmpty(replyToMessageId) && Guid.TryParse(replyToMessageId, out var replyGuid))
            replyId = new ChatMessageId(replyGuid);
        var cmd = new SendChatMessageCommand(
            new ChatGroupId(groupIdGuid),
            new UserId(uid),
            content ?? "",
            replyId);
        try
        {
            var messageId = await mediator.Send(cmd);
            var msg = await chatMessageQuery.GetByIdAsync(messageId);
            if (msg != null)
            {
                var dto = new ChatMessagePushDto
                {
                    Id = msg.Id.ToString(),
                    ChatGroupId = msg.ChatGroupId,
                    SenderId = msg.SenderId.Id,
                    SenderName = msg.SenderName,
                    Content = msg.Content,
                    ReplyToMessageId = msg.ReplyToMessageId?.ToString(),
                    CreatedAt = msg.CreatedAt,
                };
                await Clients.Group(GroupPrefix + chatGroupId).ReceiveMessageDto(dto);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.ReceiveError(ex.Message);
        }
    }
}
