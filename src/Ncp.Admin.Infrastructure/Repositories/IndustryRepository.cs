using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 行业仓储接口
/// </summary>
public interface IIndustryRepository : IRepository<Industry, IndustryId> { }

/// <summary>
/// 行业仓储实现
/// </summary>
public class IndustryRepository(ApplicationDbContext context)
    : RepositoryBase<Industry, IndustryId, ApplicationDbContext>(context), IIndustryRepository { }
