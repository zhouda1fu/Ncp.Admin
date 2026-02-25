using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 聊天消息查询 DTO
/// </summary>
public record ChatMessageQueryDto(
    ChatMessageId Id,
    string ChatGroupId,
    UserId SenderId,
    string Content,
    ChatMessageId? ReplyToMessageId,
    DateTimeOffset CreatedAt,
    string SenderName);

/// <summary>
/// 聊天消息分页查询入参
/// </summary>
public class ChatMessageQueryInput : PageRequest
{
    /// <summary>
    /// 聊天组ID
    /// </summary>
    public Guid ChatGroupId { get; set; }
}

/// <summary>
/// 聊天消息查询服务
/// </summary>
public class ChatMessageQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询一条消息
    /// </summary>
    public async Task<ChatMessageQueryDto?> GetByIdAsync(ChatMessageId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Join(dbContext.Users, m => m.SenderId, u => u.Id, (m, u) => new { m, u })
            .Select(x => new ChatMessageQueryDto(
                x.m.Id,
                x.m.ChatGroupId.ToString(),
                x.m.SenderId,
                x.m.Content,
                x.m.ReplyToMessageId,
                x.m.CreatedAt,
                string.IsNullOrEmpty(x.u.RealName) ? x.u.Name : x.u.RealName))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询某聊天组的消息（按时间倒序，即最新在前）
    /// </summary>
    public async Task<PagedData<ChatMessageQueryDto>> GetPagedByGroupIdAsync(
        ChatMessageQueryInput input,
        CancellationToken cancellationToken = default)
    {
        var groupId = new ChatGroupId(input.ChatGroupId);
        var query = dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.ChatGroupId == groupId)
            .OrderByDescending(m => m.CreatedAt)
            .Join(dbContext.Users, m => m.SenderId, u => u.Id, (m, u) => new { m, u })
            .Select(x => new ChatMessageQueryDto(
                x.m.Id,
                x.m.ChatGroupId.ToString(),
                x.m.SenderId,
                x.m.Content,
                x.m.ReplyToMessageId,
                x.m.CreatedAt,
                string.IsNullOrEmpty(x.u.RealName) ? x.u.Name : x.u.RealName));
        return await query.ToPagedDataAsync(input, cancellationToken);
    }
}
