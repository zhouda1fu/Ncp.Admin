using Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 聊天消息仓储接口
/// </summary>
public interface IChatMessageRepository : IRepository<ChatMessage, ChatMessageId> { }

/// <summary>
/// 聊天消息仓储实现
/// </summary>
public class ChatMessageRepository(ApplicationDbContext context)
    : RepositoryBase<ChatMessage, ChatMessageId, ApplicationDbContext>(context), IChatMessageRepository { }
