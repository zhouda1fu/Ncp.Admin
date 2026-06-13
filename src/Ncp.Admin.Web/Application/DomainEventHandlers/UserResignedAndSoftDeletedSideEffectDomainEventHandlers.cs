using MediatR;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;
namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 用户首次标记离职时：清除其作为部门负责人的关联。
/// </summary>
public sealed class UserResignedDomainEventHandlerForClearDeptResponsibleUser(IMediator mediator)
    : IDomainEventHandler<UserResignedDomainEvent>
{
    public Task Handle(UserResignedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        mediator.Send(new ClearUserAsDeptResponsibleUserCommand(domainEvent.UserId), cancellationToken);
}

/// <summary>
/// 用户软删除时：清除其作为部门负责人的关联。
/// </summary>
public sealed class UserSoftDeletedDomainEventHandlerForClearDeptResponsibleUser(IMediator mediator)
    : IDomainEventHandler<UserSoftDeletedDomainEvent>
{
    public Task Handle(UserSoftDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        mediator.Send(new ClearUserAsDeptResponsibleUserCommand(domainEvent.UserId), cancellationToken);
}

/// <summary>
/// 用户编辑时取消部门负责人身份：清除其负责人关联。
/// </summary>
public sealed class UserDeptResponsibleUserClearRequestedDomainEventHandlerForClearDeptResponsibleUser(IMediator mediator)
    : IDomainEventHandler<UserDeptResponsibleUserClearRequestedDomainEvent>
{
    public Task Handle(UserDeptResponsibleUserClearRequestedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        mediator.Send(new ClearUserAsDeptResponsibleUserCommand(domainEvent.UserId), cancellationToken);
}

