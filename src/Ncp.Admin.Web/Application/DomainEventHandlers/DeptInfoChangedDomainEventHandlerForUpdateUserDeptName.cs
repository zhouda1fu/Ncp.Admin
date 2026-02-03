using Ncp.Admin.Domain.DomainEvents.DeptEvents;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 部门信息变更领域事件处理器 - 用于更新用户部门名称
/// </summary>
public class DeptInfoChangedDomainEventHandlerForUpdateUserDeptName(IUserRepository userRepository) : IDomainEventHandler<DeptInfoChangedDomainEvent>
{
    public async Task Handle(DeptInfoChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dept = domainEvent.Dept;

        await userRepository.BulkUpdateUserDeptNamesAsync(dept.Id, dept.Name, cancellationToken);
    }
}
