using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 组织架构仓储接口
/// </summary>
public interface IOrganizationUnitRepository : IRepository<OrganizationUnit, OrganizationUnitId> { }

/// <summary>
/// 组织架构仓储实现
/// </summary>
public class OrganizationUnitRepository(ApplicationDbContext context) : RepositoryBase<OrganizationUnit, OrganizationUnitId, ApplicationDbContext>(context), IOrganizationUnitRepository { }

