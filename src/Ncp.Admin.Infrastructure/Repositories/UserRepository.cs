using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<IReadOnlyList<UserId>> GetActiveUserIdsByRolesAndDeptsAsync(
        IReadOnlyCollection<RoleId> roleIds,
        IReadOnlyCollection<DeptId> deptIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<long>> GetActiveUserIdValuesByPermissionAsync(
        string permissionCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<long>> GetActiveUserIdValuesByAnyPermissionAsync(
        IReadOnlyCollection<string> permissionCodes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量更新指定用户在指定角色下的角色名称（用于角色信息变更时同步冗余的 RoleName）
    /// </summary>
    Task BulkUpdateUserRoleNamesAsync(IEnumerable<UserId> userIds, RoleId roleId, string newRoleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量更新指定部门下所有用户的部门名称（用于部门信息变更时同步冗余的 DeptName）
    /// </summary>
    Task BulkUpdateUserDeptNamesAsync(DeptId deptId, string newDeptName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量更新指定岗位下所有用户的岗位名称（用于岗位信息变更时同步冗余的 PositionName）
    /// </summary>
    Task BulkUpdateUserPositionNamesAsync(PositionId positionId, string newPositionName, CancellationToken cancellationToken = default);

    Task<(bool Found, DeptId DeptId)> TryGetUserDeptAsync(
        UserId userId,
        CancellationToken cancellationToken = default);
}

public class UserRepository(ApplicationDbContext context) : RepositoryBase<User, UserId, ApplicationDbContext>(context), IUserRepository
{
    public async Task<IReadOnlyList<UserId>> GetActiveUserIdsByRolesAndDeptsAsync(
        IReadOnlyCollection<RoleId> roleIds,
        IReadOnlyCollection<DeptId> deptIds,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .Where(x => x.IsActive && x.Status == 1 && !x.IsResigned)
            .Where(x => x.Dept != null && deptIds.Contains(x.Dept.DeptId))
            .Where(x => x.Roles.Any(r => roleIds.Contains(r.RoleId)))
            .Select(x => x.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<long>> GetActiveUserIdValuesByPermissionAsync(
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        return await (
                from user in context.Users.AsNoTracking()
                join userRole in context.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                join role in context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                join permission in context.RolePermissions.AsNoTracking() on role.Id equals permission.RoleId
                where !user.IsDeleted
                      && !user.IsResigned
                      && !role.IsDeleted
                      && role.IsActive
                      && permission.PermissionCode == permissionCode
                select user.Id.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<long>> GetActiveUserIdValuesByAnyPermissionAsync(
        IReadOnlyCollection<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        if (permissionCodes.Count == 0)
            return [];

        var codeSet = permissionCodes.ToHashSet(StringComparer.Ordinal);
        return await (
                from user in context.Users.AsNoTracking()
                join userRole in context.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                join role in context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                join permission in context.RolePermissions.AsNoTracking() on role.Id equals permission.RoleId
                where !user.IsDeleted
                      && !user.IsResigned
                      && !role.IsDeleted
                      && role.IsActive
                      && codeSet.Contains(permission.PermissionCode)
                select user.Id.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task BulkUpdateUserRoleNamesAsync(IEnumerable<UserId> userIds, RoleId roleId, string newRoleName, CancellationToken cancellationToken = default)
    {
        var list = userIds as IReadOnlyList<UserId> ?? userIds.ToList();
        if (list.Count == 0)
        {
            return;
        }

        await context.UserRoles
            .Where(ur => list.Contains(ur.UserId) && ur.RoleId == roleId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(ur => ur.RoleName, newRoleName),
                cancellationToken);
    }

    public async Task BulkUpdateUserDeptNamesAsync(DeptId deptId, string newDeptName, CancellationToken cancellationToken = default)
    {
        await context.UserDepts
            .Where(ud => ud.DeptId == deptId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(ud => ud.DeptName, newDeptName),
                cancellationToken);
    }

    public async Task BulkUpdateUserPositionNamesAsync(PositionId positionId, string newPositionName, CancellationToken cancellationToken = default)
    {
        await context.UserPositions
            .Where(up => up.PositionId == positionId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(up => up.PositionName, newPositionName),
                cancellationToken);
    }

    public async Task<(bool Found, DeptId DeptId)> TryGetUserDeptAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var row = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, DeptId = u.Dept != null ? u.Dept.DeptId : DeptId.Unassigned })
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? (false, DeptId.Unassigned) : (true, row.DeptId);
    }
}

