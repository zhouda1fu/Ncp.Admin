using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.TaskAggregate;

/// <summary>
/// 项目任务评论ID（强类型ID）
/// </summary>
public partial record ProjectTaskCommentId : IGuidStronglyTypedId;

/// <summary>
/// 项目任务评论（聚合内实体）
/// </summary>
public class ProjectTaskComment : Entity<ProjectTaskCommentId>
{
    protected ProjectTaskComment() { }

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

    internal ProjectTaskComment(string content, UserId authorId)
    {
        Content = content;
        AuthorId = authorId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
