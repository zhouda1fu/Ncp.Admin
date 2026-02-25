using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 联系组仓储接口
/// </summary>
public interface IContactGroupRepository : IRepository<ContactGroup, ContactGroupId> { }

/// <summary>
/// 联系组仓储实现
/// </summary>
public class ContactGroupRepository(ApplicationDbContext context)
    : RepositoryBase<ContactGroup, ContactGroupId, ApplicationDbContext>(context), IContactGroupRepository { }
