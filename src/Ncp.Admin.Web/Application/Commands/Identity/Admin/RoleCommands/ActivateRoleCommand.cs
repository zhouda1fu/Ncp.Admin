using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.RoleCommands;

/// <summary>
/// 激活角色命令
/// </summary>
/// <param name="RoleId">角色ID</param>
public record ActivateRoleCommand(RoleId RoleId) : ICommand;

/// <summary>
/// 激活角色命令验证器
/// </summary>
public class ActivateRoleCommandValidator : AbstractValidator<ActivateRoleCommand>
{
    public ActivateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

/// <summary>
/// 激活角色命令处理器
/// </summary>
public class ActivateRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<ActivateRoleCommand>
{
    public async Task Handle(ActivateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(request.RoleId, cancellationToken) ??
                   throw new KnownException($"未找到角色，RoleId = {request.RoleId}");
        role.Activate();
    }
}
