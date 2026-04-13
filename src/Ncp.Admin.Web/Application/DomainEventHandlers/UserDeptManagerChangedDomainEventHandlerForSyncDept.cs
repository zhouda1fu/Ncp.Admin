using MediatR;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 用户部门主管标识变更领域事件处理器 - 经命令同步更新 Dept 聚合的 ManagerId
/// </summary>
public class UserDeptManagerChangedDomainEventHandlerForSyncDept(IMediator mediator)
    : IDomainEventHandler<UserDeptManagerChangedDomainEvent>
{
    public async Task Handle(UserDeptManagerChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new SyncDeptManagerFromUserDeptChangeCommand(domainEvent.UserId, domainEvent.DeptId, domainEvent.IsDeptManager),
            cancellationToken);
    }
}
