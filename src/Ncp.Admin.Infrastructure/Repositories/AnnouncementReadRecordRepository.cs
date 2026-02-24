using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 公告已读记录仓储接口
/// </summary>
public interface IAnnouncementReadRecordRepository : IRepository<AnnouncementReadRecord, AnnouncementReadRecordId>
{
    /// <summary>
    /// 判断指定用户是否已读指定公告
    /// </summary>
    /// <param name="announcementId">公告ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已读返回 true</returns>
    Task<bool> ExistsAsync(AnnouncementId announcementId, UserId userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// 在给定公告ID集合中，返回该用户已读的公告ID列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="announcementIds">待检查的公告ID集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已读的公告ID列表</returns>
    Task<IReadOnlyList<AnnouncementId>> GetReadAnnouncementIdsAsync(UserId userId, IEnumerable<AnnouncementId> announcementIds, CancellationToken cancellationToken = default);
}

/// <summary>
/// 公告已读记录仓储实现
/// </summary>
public class AnnouncementReadRecordRepository(ApplicationDbContext context)
    : RepositoryBase<AnnouncementReadRecord, AnnouncementReadRecordId, ApplicationDbContext>(context), IAnnouncementReadRecordRepository
{
    /// <inheritdoc />
    public async Task<bool> ExistsAsync(AnnouncementId announcementId, UserId userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.AnnouncementReadRecords
            .AnyAsync(r => r.AnnouncementId == announcementId && r.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AnnouncementId>> GetReadAnnouncementIdsAsync(UserId userId, IEnumerable<AnnouncementId> announcementIds, CancellationToken cancellationToken = default)
    {
        var ids = announcementIds.ToList();
        if (ids.Count == 0) return [];
        return await DbContext.AnnouncementReadRecords
            .Where(r => r.UserId == userId && ids.Contains(r.AnnouncementId))
            .Select(r => r.AnnouncementId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
