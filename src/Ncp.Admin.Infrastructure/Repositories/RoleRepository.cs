using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<Role, RoleId>
{
    Task<IReadOnlyList<Role>> GetByIdsAsync(
        IEnumerable<RoleId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量加载角色及其权限（不含数据权限部门），用于预设包同步权限。
    /// </summary>
    Task<IReadOnlyList<Role>> GetByIdsForPermissionSyncAsync(
        IEnumerable<RoleId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 为多个角色批量追加缺失权限（不加载已有权限明细，适合大批量同步）。
    /// </summary>
    Task AppendMissingPermissionsAsync(
        IReadOnlyList<RoleId> roleIds,
        IReadOnlyList<RolePermission> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从多个角色批量移除指定权限码，忽略角色当前未包含的权限。
    /// </summary>
    Task RemovePermissionsAsync(
        IReadOnlyList<RoleId> roleIds,
        IReadOnlyList<string> permissionCodes,
        CancellationToken cancellationToken = default);
}

public class RoleRepository(ApplicationDbContext context)
    : RepositoryBase<Role, RoleId, ApplicationDbContext>(context),
        IRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetByIdsAsync(
        IEnumerable<RoleId> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = (ids ?? []).Where(id => id != RoleId.Unassigned).Distinct().ToList();
        if (idList.Count == 0)
            return [];

        return await context.Roles
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetByIdsForPermissionSyncAsync(
        IEnumerable<RoleId> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = (ids ?? []).Where(id => id != RoleId.Unassigned).Distinct().ToList();
        if (idList.Count == 0)
            return [];

        return await context.Roles
            .IgnoreAutoIncludes()
            .AsSplitQuery()
            .Include(r => r.Permissions)
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AppendMissingPermissionsAsync(
        IReadOnlyList<RoleId> roleIds,
        IReadOnlyList<RolePermission> permissions,
        CancellationToken cancellationToken = default)
    {
        var idList = (roleIds ?? [])
            .Where(id => id != RoleId.Unassigned)
            .Distinct()
            .ToList();
        if (idList.Count == 0)
            return;

        var permissionList = (permissions ?? [])
            .Where(p => !string.IsNullOrWhiteSpace(p.PermissionCode))
            .ToList();
        if (permissionList.Count == 0)
            return;

        var existingRoleIds = await context.Roles
            .IgnoreAutoIncludes()
            .Where(r => idList.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);
        if (existingRoleIds.Count != idList.Count)
            throw new KnownException("部分角色不存在或已删除，请刷新后重试");

        var permissionCodes = permissionList
            .Select(p => p.PermissionCode)
            .ToHashSet(StringComparer.Ordinal);

        var existingPairs = await context.Set<RolePermission>()
            .AsNoTracking()
            .Where(rp => idList.Contains(rp.RoleId) && permissionCodes.Contains(rp.PermissionCode))
            .Select(rp => new { rp.RoleId, rp.PermissionCode })
            .ToListAsync(cancellationToken);

        var existingSet = existingPairs
            .Select(p => (p.RoleId, p.PermissionCode))
            .ToHashSet();

        var roles = await context.Roles
            .IgnoreAutoIncludes()
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);

        foreach (var role in roles)
        {
            foreach (var permission in permissionList)
            {
                if (existingSet.Contains((role.Id, permission.PermissionCode)))
                    continue;

                role.Permissions.Add(new RolePermission(
                    permission.PermissionCode,
                    permission.PermissionName,
                    permission.PermissionDescription));
            }
        }
    }

    public async Task RemovePermissionsAsync(
        IReadOnlyList<RoleId> roleIds,
        IReadOnlyList<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        var idList = (roleIds ?? [])
            .Where(id => id != RoleId.Unassigned)
            .Distinct()
            .ToList();
        if (idList.Count == 0)
            return;

        var codeList = (permissionCodes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (codeList.Count == 0)
            return;

        var existingRoleIds = await context.Roles
            .IgnoreAutoIncludes()
            .Where(r => idList.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);
        if (existingRoleIds.Count != idList.Count)
            throw new KnownException("部分角色不存在或已删除，请刷新后重试");

        var roles = await context.Roles
            .IgnoreAutoIncludes()
            .Include(r => r.Permissions)
            .AsSplitQuery()
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);

        foreach (var role in roles)
        {
            role.RemovePermissions(codeList);
        }
    }
}
