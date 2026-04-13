using MediatR;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 用户离职或删除领域事件处理器 - 经命令清除该用户作为部门主管的关联
/// </summary>
public class UserResignedOrDeletedDomainEventHandlerForClearDeptManager(IMediator mediator)
    : IDomainEventHandler<UserResignedOrDeletedDomainEvent>
{
    public async Task Handle(UserResignedOrDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await mediator.Send(new ClearUserAsDeptManagerCommand(domainEvent.UserId), cancellationToken);
    }
}
