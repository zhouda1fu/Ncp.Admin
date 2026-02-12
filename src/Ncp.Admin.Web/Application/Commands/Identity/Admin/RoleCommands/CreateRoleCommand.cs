using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.RoleCommands;


public record CreateRoleCommand(string Name, string Description, IEnumerable<string> PermissionCodes, DataScope DataScope = DataScope.All) : ICommand<RoleId>;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(RoleQuery roleQuery)
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("角色名称不能为空");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("角色描述长度不能超过200个字符");
        RuleFor(r => r.Name).MustAsync(async (n, ct) => !await roleQuery.DoesRoleExist(n, ct))
            .WithMessage(r => $"该角色已存在，Name={r.Name}");
    }
}

public class CreateRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<CreateRoleCommand, RoleId>
{
    public async Task<RoleId> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var permissions = request.PermissionCodes.Select(perm =>
        {
            var (name, description) = PermissionMapper.GetPermissionInfo(perm);
            return new RolePermission(perm, name, description);
        });

        var role = new Role(request.Name, request.Description, permissions, request.DataScope);

        await roleRepository.AddAsync(role, cancellationToken);

        return role.Id;
    }
}
