using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 任务评论查询 DTO
/// </summary>
/// <param name="Id">评论 ID</param>
/// <param name="Content">评论内容</param>
/// <param name="AuthorId">作者用户 ID</param>
/// <param name="CreatedAt">创建时间</param>
public record TaskCommentQueryDto(ProjectTaskCommentId Id, string Content, UserId AuthorId, DateTimeOffset CreatedAt);

/// <summary>
/// 任务查询 DTO（含评论列表）
/// </summary>
/// <param name="Id">任务 ID</param>
/// <param name="ProjectId">所属项目 ID</param>
/// <param name="Title">标题</param>
/// <param name="Description">描述</param>
/// <param name="AssigneeId">负责人，未指定为 null</param>
/// <param name="DueDate">截止日期</param>
/// <param name="Status">状态</param>
/// <param name="SortOrder">排序号</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="Comments">评论列表</param>
public record TaskQueryDto(
    ProjectTaskId Id,
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
    public ProjectTaskStatus? Status { get; set; }
}

/// <summary>
/// 任务查询服务
/// </summary>
public class TaskQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询任务（含评论）
    /// </summary>
    public async Task<TaskQueryDto?> GetByIdAsync(ProjectTaskId id, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.ProjectTasks
            .AsNoTracking()
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null) return null;
        return new TaskQueryDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.AssigneeId == new UserId(0) ? null : task.AssigneeId,
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
        IQueryable<ProjectTask> query = dbContext.ProjectTasks.AsNoTracking().Include(t => t.Comments);
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
                t.AssigneeId == new UserId(0) ? null : t.AssigneeId,
                t.DueDate,
                (int)t.Status,
                t.SortOrder,
                t.CreatedAt,
                t.Comments.Select(c => new TaskCommentQueryDto(c.Id, c.Content, c.AuthorId, c.CreatedAt)).ToList()))
            .ToPagedDataAsync(input, cancellationToken);
        return list;
    }
}
