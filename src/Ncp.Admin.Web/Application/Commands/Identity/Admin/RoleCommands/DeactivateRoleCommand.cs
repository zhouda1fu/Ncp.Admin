using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.RoleCommands;

/// <summary>
/// 停用角色命令
/// </summary>
/// <param name="RoleId">角色ID</param>
public record DeactivateRoleCommand(RoleId RoleId) : ICommand;

/// <summary>
/// 停用角色命令验证器
/// </summary>
public class DeactivateRoleCommandValidator : AbstractValidator<DeactivateRoleCommand>
{
    public DeactivateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

/// <summary>
/// 停用角色命令处理器
/// </summary>
public class DeactivateRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<DeactivateRoleCommand>
{
    public async Task Handle(DeactivateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(request.RoleId, cancellationToken) ??
                   throw new KnownException($"未找到角色，RoleId = {request.RoleId}");
        role.Deactivate();
    }
}
