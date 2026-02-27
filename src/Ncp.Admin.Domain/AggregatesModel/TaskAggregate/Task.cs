using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.TaskAggregate;

/// <summary>
/// 任务ID（强类型ID）
/// </summary>
public partial record TaskId : IGuidStronglyTypedId;

/// <summary>
/// 任务状态
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// 待办
    /// </summary>
    Todo = 0,
    /// <summary>
    /// 进行中
    /// </summary>
    InProgress = 1,
    /// <summary>
    /// 已完成
    /// </summary>
    Done = 2,
    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 3,
}

/// <summary>
/// 任务聚合根，含评论列表，属于某项目
/// </summary>
public class Task : Entity<TaskId>, IAggregateRoot
{
    private readonly List<TaskComment> _comments = [];

    protected Task() { }

    /// <summary>
    /// 所属项目ID
    /// </summary>
    public ProjectId ProjectId { get; private set; } = default!;
    /// <summary>
    /// 任务标题
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 任务描述（可选）
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// 负责人用户ID（可选）
    /// </summary>
    public UserId? AssigneeId { get; private set; }
    /// <summary>
    /// 截止日期（可选）
    /// </summary>
    public DateOnly? DueDate { get; private set; }
    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskStatus Status { get; private set; }
    /// <summary>
    /// 排序号（看板内排序）
    /// </summary>
    public int SortOrder { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 评论列表（只读）
    /// </summary>
    public IReadOnlyList<TaskComment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// 创建任务
    /// </summary>
    public Task(ProjectId projectId, string title, string? description = null, UserId? assigneeId = null, DateOnly? dueDate = null, int sortOrder = 0)
    {
        ProjectId = projectId;
        Title = title ;
        Description = description;
        AssigneeId = assigneeId;
        DueDate = dueDate;
        SortOrder = sortOrder;
        Status = TaskStatus.Todo;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新任务信息
    /// </summary>
    public void Update(string title, string? description, UserId? assigneeId, DateOnly? dueDate)
    {
        Title = title ;
        Description = description;
        AssigneeId = assigneeId;
        DueDate = dueDate;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 更新排序号
    /// </summary>
    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 设置任务状态
    /// </summary>
    public void SetStatus(TaskStatus status)
    {
        Status = status;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 添加评论
    /// </summary>
    public void AddComment(string content, UserId authorId)
    {
        _comments.Add(new TaskComment(content, authorId));
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
