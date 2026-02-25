using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 联系人仓储接口
/// </summary>
public interface IContactRepository : IRepository<Contact, ContactId> { }

/// <summary>
/// 联系人仓储实现
/// </summary>
public class ContactRepository(ApplicationDbContext context)
    : RepositoryBase<Contact, ContactId, ApplicationDbContext>(context), IContactRepository { }
