using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.RoleCommands;

/// <summary>
/// 批量调整角色权限的操作类型。
/// </summary>
public enum RolePermissionBatchOperation
{
    Add = 0,
    Remove = 1,
}

/// <summary>
/// 批量调整角色权限命令。
/// </summary>
/// <param name="RoleIds">角色标识列表。</param>
/// <param name="Operation">操作类型。</param>
/// <param name="PermissionCodes">权限码列表。</param>
public record BatchUpdateRolePermissionsCommand(
    IReadOnlyList<RoleId> RoleIds,
    RolePermissionBatchOperation Operation,
    IReadOnlyList<string> PermissionCodes) : ICommand;

/// <summary>
/// 批量调整角色权限命令验证器。
/// </summary>
public class BatchUpdateRolePermissionsCommandValidator : AbstractValidator<BatchUpdateRolePermissionsCommand>
{
    public BatchUpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleIds).NotEmpty().WithMessage("请至少选择一个角色");
        RuleFor(x => x.Operation).IsInEnum();
        RuleFor(x => x.PermissionCodes).NotEmpty().WithMessage("请至少选择一个权限");
    }
}

/// <summary>
/// 批量调整角色权限命令处理器。
/// </summary>
public class BatchUpdateRolePermissionsCommandHandler(
    IRoleRepository roleRepository,
    UserQuery userQuery,
    IMemoryCache memoryCache)
    : ICommandHandler<BatchUpdateRolePermissionsCommand>
{
    public async Task Handle(BatchUpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var roleIds = (request.RoleIds ?? [])
            .Where(id => id != RoleId.Unassigned)
            .Distinct()
            .ToList();
        if (roleIds.Count == 0)
            throw new KnownException("请至少选择一个角色");

        var permissionCodes = PermissionCodeValidator.NormalizeAndValidate(request.PermissionCodes);
        if (permissionCodes.Count == 0)
            throw new KnownException("请至少选择一个权限");

        if (request.Operation == RolePermissionBatchOperation.Add)
        {
            var permissions = permissionCodes
                .Select(code =>
                {
                    var (name, description) = PermissionMapper.GetPermissionInfo(code);
                    return new RolePermission(code, name, description);
                })
                .ToList();

            await roleRepository.AppendMissingPermissionsAsync(roleIds, permissions, cancellationToken);
        }
        else
        {
            await roleRepository.RemovePermissionsAsync(roleIds, permissionCodes, cancellationToken);
        }

        foreach (var roleId in roleIds)
        {
            memoryCache.Remove(RoleQuery.GetRoleCacheKey(roleId));
        }

        var affectedUserIds = new HashSet<UserId>();
        foreach (var roleId in roleIds)
        {
            var userIds = await userQuery.GetUserIdsByRoleIdAsync(roleId, cancellationToken);
            foreach (var userId in userIds)
            {
                affectedUserIds.Add(userId);
            }
        }

        foreach (var userId in affectedUserIds)
        {
            memoryCache.Remove(PermissionClaimsTransformation.GetPermissionCodesCacheKey(userId));
        }
    }
}
