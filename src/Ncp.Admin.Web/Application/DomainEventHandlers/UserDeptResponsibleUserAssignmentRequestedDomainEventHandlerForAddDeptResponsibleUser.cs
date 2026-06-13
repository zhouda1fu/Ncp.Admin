using MediatR;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 用户创建快捷入口请求部门负责人关系时，转为部门命令处理，避免用户命令处理器直接修改部门聚合。
/// </summary>
public sealed class UserDeptResponsibleUserAssignmentRequestedDomainEventHandlerForAddDeptResponsibleUser(
    IMediator mediator) : IDomainEventHandler<UserDeptResponsibleUserAssignmentRequestedDomainEvent>
{
    public Task Handle(
        UserDeptResponsibleUserAssignmentRequestedDomainEvent domainEvent,
        CancellationToken cancellationToken) =>
        mediator.Send(
            new AddUserAsDeptResponsibleUserCommand(
                domainEvent.DeptId,
                domainEvent.UserId,
                domainEvent.SetAsDefault),
            cancellationToken);
}
