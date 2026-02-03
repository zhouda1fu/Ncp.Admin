using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User, UserId>
{
    /// <summary>
    /// 批量更新指定用户在指定角色下的角色名称（用于角色信息变更时同步冗余的 RoleName）
    /// </summary>
    Task BulkUpdateUserRoleNamesAsync(IEnumerable<UserId> userIds, RoleId roleId, string newRoleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量更新指定部门下所有用户的部门名称（用于部门信息变更时同步冗余的 DeptName）
    /// </summary>
    Task BulkUpdateUserDeptNamesAsync(DeptId deptId, string newDeptName, CancellationToken cancellationToken = default);
}

public class UserRepository(ApplicationDbContext context) : RepositoryBase<User, UserId, ApplicationDbContext>(context), IUserRepository
{
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
}

