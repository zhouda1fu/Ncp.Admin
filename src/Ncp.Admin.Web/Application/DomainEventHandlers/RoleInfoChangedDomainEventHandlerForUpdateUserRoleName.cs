using Ncp.Admin.Domain.DomainEvents.RoleEvents;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 角色信息变更领域事件处理器 - 用于更新用户角色名称
/// </summary>
public class RoleInfoChangedDomainEventHandlerForUpdateUserRoleName(IMediator mediator, UserQuery userQuery) : IDomainEventHandler<RoleInfoChangedDomainEvent>
{
    public async Task Handle(RoleInfoChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var role = domainEvent.Role;
        var roleId = role.Id;
        var newRoleName = role.Name;

        // 查询所有拥有该角色的用户ID
        var userIds = await userQuery.GetUserIdsByRoleIdAsync(roleId, cancellationToken);

        // 通过批量命令一次更新所有用户的角色名称，避免 N+1 性能问题
        await mediator.Send(new BatchUpdateUserRoleNamesCommand(userIds, roleId, newRoleName), cancellationToken);
    }
}
