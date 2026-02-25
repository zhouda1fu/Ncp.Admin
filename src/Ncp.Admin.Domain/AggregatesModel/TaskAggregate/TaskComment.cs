using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.TaskAggregate;

/// <summary>
/// 任务评论ID（强类型ID）
/// </summary>
public partial record TaskCommentId : IGuidStronglyTypedId;

/// <summary>
/// 任务评论（聚合内实体）
/// </summary>
public class TaskComment : Entity<TaskCommentId>
{
    protected TaskComment() { }

    /// <summary>
    /// 评论内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;
    /// <summary>
    /// 评论人用户ID
    /// </summary>
    public UserId AuthorId { get; private set; } = default!;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    internal TaskComment(string content, UserId authorId)
    {
        Content = content ?? string.Empty;
        AuthorId = authorId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
