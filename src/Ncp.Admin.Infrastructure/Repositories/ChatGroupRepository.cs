using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 聊天组仓储接口
/// </summary>
public interface IChatGroupRepository : IRepository<ChatGroup, ChatGroupId> { }

/// <summary>
/// 聊天组仓储实现
/// </summary>
public class ChatGroupRepository(ApplicationDbContext context)
    : RepositoryBase<ChatGroup, ChatGroupId, ApplicationDbContext>(context), IChatGroupRepository { }
