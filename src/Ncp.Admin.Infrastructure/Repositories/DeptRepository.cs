using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 部门仓储接口
/// </summary>
public interface IDeptRepository : IRepository<Dept, DeptId>
{
    Task<Dept?> GetWithResponsibleUsersAsync(DeptId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Dept>> GetDeptsWithResponsibleUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveChildrenAsync(DeptId id, CancellationToken cancellationToken = default);
    Task<bool> HasUsersAsync(DeptId id, CancellationToken cancellationToken = default);
    Task<bool> HasActivePositionsAsync(DeptId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Dept>> GetActiveSiblingsAsync(DeptId parentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 部门仓储实现
/// </summary>
public class DeptRepository(ApplicationDbContext context) : RepositoryBase<Dept, DeptId, ApplicationDbContext>(context), IDeptRepository
{
    public Task<Dept?> GetWithResponsibleUsersAsync(DeptId id, CancellationToken cancellationToken = default)
    {
        return context.Depts
            .Include(d => d.ResponsibleUsers)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Dept>> GetDeptsWithResponsibleUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var deptIds = await context.DeptResponsibleUsers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.DeptId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (deptIds.Count == 0)
            return [];

        return await context.Depts
            .Include(d => d.ResponsibleUsers)
            .Where(d => deptIds.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasActiveChildrenAsync(DeptId id, CancellationToken cancellationToken = default)
    {
        return context.Depts.AnyAsync(d => d.ParentId == id && !d.IsDeleted, cancellationToken);
    }

    public Task<bool> HasUsersAsync(DeptId id, CancellationToken cancellationToken = default)
    {
        return context.UserDepts.AnyAsync(ud => ud.DeptId == id, cancellationToken);
    }

    public Task<bool> HasActivePositionsAsync(DeptId id, CancellationToken cancellationToken = default)
    {
        return context.Positions.AnyAsync(p => p.DeptId == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Dept>> GetActiveSiblingsAsync(DeptId parentId, CancellationToken cancellationToken = default)
    {
        return await context.Depts
            .Where(d => d.ParentId == parentId && !d.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
