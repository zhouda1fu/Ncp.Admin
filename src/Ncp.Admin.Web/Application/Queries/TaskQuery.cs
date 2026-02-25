using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 任务评论查询 DTO
/// </summary>
public record TaskCommentQueryDto(TaskCommentId Id, string Content, UserId AuthorId, DateTimeOffset CreatedAt);

/// <summary>
/// 任务查询 DTO（含评论列表）
/// </summary>
public record TaskQueryDto(
    TaskId Id,
    ProjectId ProjectId,
    string Title,
    string? Description,
    UserId? AssigneeId,
    DateOnly? DueDate,
    int Status,
    int SortOrder,
    DateTimeOffset CreatedAt,
    List<TaskCommentQueryDto> Comments);

/// <summary>
/// 任务分页查询入参
/// </summary>
public class TaskQueryInput : PageRequest
{
    /// <summary>
    /// 项目ID筛选
    /// </summary>
    public ProjectId? ProjectId { get; set; }
    /// <summary>
    /// 状态筛选
    /// </summary>
    public Ncp.Admin.Domain.AggregatesModel.TaskAggregate.TaskStatus? Status { get; set; }
}

/// <summary>
/// 任务查询服务
/// </summary>
public class TaskQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询任务（含评论）
    /// </summary>
    public async Task<TaskQueryDto?> GetByIdAsync(TaskId id, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Tasks
            .AsNoTracking()
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null) return null;
        return new TaskQueryDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.AssigneeId,
            task.DueDate,
            (int)task.Status,
            task.SortOrder,
            task.CreatedAt,
            task.Comments.Select(c => new TaskCommentQueryDto(c.Id, c.Content, c.AuthorId, c.CreatedAt)).ToList());
    }

    /// <summary>
    /// 分页查询任务
    /// </summary>
    public async Task<PagedData<TaskQueryDto>> GetPagedAsync(TaskQueryInput input, CancellationToken cancellationToken = default)
    {
        IQueryable<Ncp.Admin.Domain.AggregatesModel.TaskAggregate.Task> query = dbContext.Tasks.AsNoTracking().Include(t => t.Comments);
        if (input.ProjectId != null)
            query = query.Where(t => t.ProjectId == input.ProjectId);
        if (input.Status.HasValue)
            query = query.Where(t => t.Status == input.Status.Value);
        var list = await query
            .OrderBy(t => t.ProjectId)
            .ThenBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new TaskQueryDto(
                t.Id,
                t.ProjectId,
                t.Title,
                t.Description,
                t.AssigneeId,
                t.DueDate,
                (int)t.Status,
                t.SortOrder,
                t.CreatedAt,
                t.Comments.Select(c => new TaskCommentQueryDto(c.Id, c.Content, c.AuthorId, c.CreatedAt)).ToList()))
            .ToPagedDataAsync(input, cancellationToken);
        return list;
    }
}
