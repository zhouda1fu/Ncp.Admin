using Ncp.Admin.Domain.DomainEvents.UserEvents;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 用户离职或删除领域事件处理器 - 清除该用户作为部门主管的关联
/// </summary>
public class UserResignedOrDeletedDomainEventHandlerForClearDeptManager(IDeptRepository deptRepository) : IDomainEventHandler<UserResignedOrDeletedDomainEvent>
{
    public async Task Handle(UserResignedOrDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await deptRepository.ClearManagerIdForUserAsync(domainEvent.UserId, cancellationToken);
    }
}
