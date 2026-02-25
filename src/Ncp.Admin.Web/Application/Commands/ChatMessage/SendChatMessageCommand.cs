using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ChatMessage;

/// <summary>
/// 发送聊天消息命令
/// </summary>
public record SendChatMessageCommand(ChatGroupId ChatGroupId, UserId SenderId, string Content, ChatMessageId? ReplyToMessageId)
    : ICommand<ChatMessageId>;

/// <summary>
/// 发送聊天消息命令验证器
/// </summary>
public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    /// <inheritdoc />
    public SendChatMessageCommandValidator()
    {
        RuleFor(c => c.ChatGroupId).NotNull();
        RuleFor(c => c.SenderId).NotNull();
        RuleFor(c => c.Content).NotEmpty().MaximumLength(4000);
    }
}

/// <summary>
/// 发送聊天消息命令处理器
/// </summary>
public class SendChatMessageCommandHandler(
    IChatMessageRepository messageRepository,
    IChatGroupRepository groupRepository) : ICommandHandler<SendChatMessageCommand, ChatMessageId>
{
    /// <inheritdoc />
    public async Task<ChatMessageId> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetAsync(request.ChatGroupId, cancellationToken)
            ?? throw new KnownException("未找到聊天组", ErrorCodes.ChatGroupNotFound);
        if (!group.ContainsUser(request.SenderId))
            throw new KnownException("您不在该聊天组中", ErrorCodes.NotMemberOfChatGroup);
        var message = new Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate.ChatMessage(
            request.ChatGroupId, request.SenderId, request.Content, request.ReplyToMessageId);
        await messageRepository.AddAsync(message, cancellationToken);
        return message.Id;
    }
}
