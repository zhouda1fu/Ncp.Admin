using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 部门仓储接口
/// </summary>
public interface IDeptRepository : IRepository<Dept, DeptId>
{
    /// <summary>
    /// 清除指定用户作为部门主管的关联（用户离职或删除时调用）
    /// </summary>
    Task ClearManagerIdForUserAsync(UserId userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 部门仓储实现
/// </summary>
public class DeptRepository(ApplicationDbContext context) : RepositoryBase<Dept, DeptId, ApplicationDbContext>(context), IDeptRepository
{
    public async Task ClearManagerIdForUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        await context.Depts
            .Where(d => d.ManagerId == userId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(d => d.ManagerId, new UserId(0)),
                cancellationToken);
    }
}
