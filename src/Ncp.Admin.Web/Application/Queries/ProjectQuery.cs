using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 项目查询 DTO
/// </summary>
public record ProjectQueryDto(ProjectId Id, string Name, string? Description, UserId CreatorId, int Status, DateTimeOffset CreatedAt);

/// <summary>
/// 项目分页查询入参
/// </summary>
public class ProjectQueryInput : PageRequest
{
    /// <summary>
    /// 名称关键字
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 状态筛选（0 进行中 1 已归档）
    /// </summary>
    public int? Status { get; set; }
}

/// <summary>
/// 项目查询服务
/// </summary>
public class ProjectQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询项目
    /// </summary>
    public async Task<ProjectQueryDto?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProjectQueryDto(p.Id, p.Name, p.Description, p.CreatorId, (int)p.Status, p.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询项目
    /// </summary>
    public async Task<PagedData<ProjectQueryDto>> GetPagedAsync(ProjectQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Projects.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Name))
            query = query.Where(p => p.Name.Contains(input.Name));
        if (input.Status.HasValue)
            query = query.Where(p => (int)p.Status == input.Status.Value);
        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectQueryDto(p.Id, p.Name, p.Description, p.CreatorId, (int)p.Status, p.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
