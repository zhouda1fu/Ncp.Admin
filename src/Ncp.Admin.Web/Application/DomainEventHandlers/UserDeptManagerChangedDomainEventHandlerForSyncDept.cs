using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents.UserEvents;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 用户部门主管标识变更领域事件处理器 - 同步更新 Dept 聚合的 ManagerId
/// </summary>
public class UserDeptManagerChangedDomainEventHandlerForSyncDept(IDeptRepository deptRepository)
    : IDomainEventHandler<UserDeptManagerChangedDomainEvent>
{
    public async Task Handle(UserDeptManagerChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dept = await deptRepository.GetAsync(domainEvent.DeptId, cancellationToken);
        if (dept == null)
            return;

        if (domainEvent.IsDeptManager)
        {
            dept.SetManagerId(domainEvent.UserId);
        }
        else if (dept.ManagerId == domainEvent.UserId)
        {
            dept.SetManagerId(new UserId(0));
        }
    }
}
