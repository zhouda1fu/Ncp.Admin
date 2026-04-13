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
    /// 查询主管为指定用户的部门 ID 列表（用于通过聚合根清除主管关联）
    /// </summary>
    Task<IReadOnlyList<DeptId>> GetDeptIdsWhereManagerIsUserAsync(UserId userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 部门仓储实现
/// </summary>
public class DeptRepository(ApplicationDbContext context) : RepositoryBase<Dept, DeptId, ApplicationDbContext>(context), IDeptRepository
{
    public async Task<IReadOnlyList<DeptId>> GetDeptIdsWhereManagerIsUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await context.Depts
            .AsNoTracking()
            .Where(d => d.ManagerId == userId)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);
    }
}
