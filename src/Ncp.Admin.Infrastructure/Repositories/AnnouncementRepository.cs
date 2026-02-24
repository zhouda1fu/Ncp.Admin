using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 公告仓储接口
/// </summary>
public interface IAnnouncementRepository : IRepository<Announcement, AnnouncementId> { }

/// <summary>
/// 公告仓储实现
/// </summary>
public class AnnouncementRepository(ApplicationDbContext context)
    : RepositoryBase<Announcement, AnnouncementId, ApplicationDbContext>(context), IAnnouncementRepository { }
