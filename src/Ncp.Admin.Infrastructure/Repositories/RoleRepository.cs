using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<Role, RoleId> { }

public class RoleRepository(ApplicationDbContext context) : RepositoryBase<Role, RoleId, ApplicationDbContext>(context), IRoleRepository { }

