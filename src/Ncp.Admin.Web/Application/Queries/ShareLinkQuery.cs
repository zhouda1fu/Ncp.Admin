using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 按令牌查询共享链接结果（用于公开访问）
/// </summary>
public record ShareLinkByTokenDto(DocumentId DocumentId, string Title, bool Expired);

/// <summary>
/// 共享链接查询服务
/// </summary>
public class ShareLinkQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按令牌获取共享链接及文档标题（若已过期或不存在返回 null）
    /// </summary>
    public async Task<ShareLinkByTokenDto?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var link = await dbContext.ShareLinks
            .AsNoTracking()
            .Where(s => s.Token == token)
            .FirstOrDefaultAsync(cancellationToken);
        if (link == null || link.IsExpired)
            return null;
        var doc = await dbContext.Documents
            .AsNoTracking()
            .Where(d => d.Id == link.DocumentId)
            .Select(d => new { d.Id, d.Title })
            .FirstOrDefaultAsync(cancellationToken);
        if (doc == null) return null;
        return new ShareLinkByTokenDto(link.DocumentId, doc.Title, link.IsExpired);
    }
}
