using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 公告查询 DTO
/// </summary>
public record AnnouncementQueryDto(
    AnnouncementId Id,
    string Title,
    string Content,
    UserId PublisherId,
    string PublisherName,
    AnnouncementStatus Status,
    DateTimeOffset? PublishAt,
    DateTimeOffset CreatedAt,
    bool? IsRead = null);

/// <summary>
/// 公告分页查询入参
/// </summary>
public class AnnouncementQueryInput : PageRequest
{
    /// <summary>
    /// 标题关键字
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// 状态筛选
    /// </summary>
    public AnnouncementStatus? Status { get; set; }
    /// <summary>
    /// 发布人ID筛选
    /// </summary>
    public UserId? PublisherId { get; set; }
}

/// <summary>
/// 公告查询服务（支持按当前用户填充已读状态）
/// </summary>
public class AnnouncementQuery(ApplicationDbContext dbContext, IAnnouncementReadRecordRepository readRecordRepository) : IQuery
{
    /// <summary>
    /// 按 ID 查询公告详情；若传入 readerId 则填充 IsRead
    /// </summary>
    public async Task<AnnouncementQueryDto?> GetByIdAsync(AnnouncementId id, UserId? readerId, CancellationToken cancellationToken = default)
    {
        var dto = await dbContext.Announcements
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AnnouncementQueryDto(
                a.Id,
                a.Title,
                a.Content,
                a.PublisherId,
                a.PublisherName,
                a.Status,
                a.PublishAt,
                a.CreatedAt,
                (bool?)null))
            .FirstOrDefaultAsync(cancellationToken);
        if (dto != null && readerId != null)
        {
            var isRead = await readRecordRepository.ExistsAsync(id, readerId, cancellationToken);
            dto = dto with { IsRead = isRead };
        }
        return dto;
    }

    /// <summary>
    /// 分页查询公告列表；若传入 readerId 则填充每条 IsRead
    /// </summary>
    public async Task<PagedData<AnnouncementQueryDto>> GetPagedAsync(AnnouncementQueryInput input, UserId? readerId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Announcements.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Title))
            query = query.Where(a => a.Title.Contains(input.Title));
        if (input.Status.HasValue)
            query = query.Where(a => a.Status == input.Status.Value);
        if (input.PublisherId != null)
            query = query.Where(a => a.PublisherId == input.PublisherId);

        var list = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementQueryDto(
                a.Id,
                a.Title,
                a.Content,
                a.PublisherId,
                a.PublisherName,
                a.Status,
                a.PublishAt,
                a.CreatedAt,
                (bool?)null))
            .ToPagedDataAsync(input, cancellationToken);

        if (readerId != null && list.Items.Count() > 0)
        {
            var ids = list.Items.Select(x => x.Id).ToList();
            var readIds = await readRecordRepository.GetReadAnnouncementIdsAsync(readerId, ids, cancellationToken);
            var readSet = readIds.ToHashSet();
            var items = list.Items.Select(dto => dto with { IsRead = readSet.Contains(dto.Id) }).ToList();
            return new PagedData<AnnouncementQueryDto>(items, input.PageIndex, input.PageSize, (int)list.Total);
        }

        return list;
    }
}
