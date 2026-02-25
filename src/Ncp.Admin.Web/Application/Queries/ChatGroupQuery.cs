using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 聊天组查询 DTO
/// </summary>
public record ChatGroupQueryDto(
    ChatGroupId Id,
    string? Name,
    int Type,
    UserId CreatorId,
    DateTimeOffset CreatedAt,
    int MemberCount);

/// <summary>
/// 聊天组查询服务
/// </summary>
public class ChatGroupQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询聊天组
    /// </summary>
    public async Task<ChatGroupQueryDto?> GetByIdAsync(ChatGroupId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatGroups
            .AsNoTracking()
            .Include(x => x.Members)
            .Where(g => g.Id == id)
            .Select(g => new ChatGroupQueryDto(
                g.Id,
                g.Name,
                (int)g.Type,
                g.CreatorId,
                g.CreatedAt,
                g.Members.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 获取当前用户参与的所有聊天组
    /// </summary>
    public async Task<List<ChatGroupQueryDto>> GetMyGroupsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatGroups
            .AsNoTracking()
            .Include(x => x.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new ChatGroupQueryDto(
                g.Id,
                g.Name,
                (int)g.Type,
                g.CreatorId,
                g.CreatedAt,
                g.Members.Count))
            .ToListAsync(cancellationToken);
    }
}
