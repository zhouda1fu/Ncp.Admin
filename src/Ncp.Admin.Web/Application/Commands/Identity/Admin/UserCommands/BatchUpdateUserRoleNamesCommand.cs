using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 批量更新用户角色名称命令（用于角色信息变更时同步更新所有关联用户的角色名称）
/// </summary>
public record BatchUpdateUserRoleNamesCommand(
    IEnumerable<UserId> UserIds,
    RoleId RoleId,
    string NewRoleName) : ICommand;

/// <summary>
/// 批量更新用户角色名称命令处理器
/// </summary>
public class BatchUpdateUserRoleNamesCommandHandler(IUserRepository userRepository)
    : ICommandHandler<BatchUpdateUserRoleNamesCommand>
{
    public async Task Handle(BatchUpdateUserRoleNamesCommand request, CancellationToken cancellationToken)
    {
        await userRepository.BulkUpdateUserRoleNamesAsync(
            request.UserIds,
            request.RoleId,
            request.NewRoleName,
            cancellationToken);
    }
}
