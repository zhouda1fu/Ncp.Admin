using Ncp.Admin.Domain.DomainEvents.DeptEvents;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 部门信息变更领域事件处理器 - 用于更新用户部门名称
/// </summary>
public class DeptInfoChangedDomainEventHandlerForUpdateUserDeptName(IMediator mediator) : IDomainEventHandler<DeptInfoChangedDomainEvent>
{
    public async Task Handle(DeptInfoChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dept = domainEvent.Dept;

        await mediator.Send(new BatchUpdateUserDeptNamesCommand(dept.Id, dept.Name), cancellationToken);
    }
}
