using Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 共享链接仓储接口
/// </summary>
public interface IShareLinkRepository : IRepository<ShareLink, ShareLinkId> { }

/// <summary>
/// 共享链接仓储实现
/// </summary>
public class ShareLinkRepository(ApplicationDbContext context)
    : RepositoryBase<ShareLink, ShareLinkId, ApplicationDbContext>(context), IShareLinkRepository { }
