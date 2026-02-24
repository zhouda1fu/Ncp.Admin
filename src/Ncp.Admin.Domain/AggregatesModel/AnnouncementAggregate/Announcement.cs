using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;

/// <summary>
/// 公告ID（强类型ID）
/// </summary>
public partial record AnnouncementId : IGuidStronglyTypedId;

/// <summary>
/// 公告状态
/// </summary>
public enum AnnouncementStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,
    /// <summary>
    /// 已发布
    /// </summary>
    Published = 1,
}

/// <summary>
/// 公告聚合根，用于公司内部公告的创建、编辑与发布
/// </summary>
public class Announcement : Entity<AnnouncementId>, IAggregateRoot
{
    protected Announcement() { }

    /// <summary>
    /// 公告标题，最大长度 200
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>
    /// 公告正文内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;
    /// <summary>
    /// 发布人用户ID
    /// </summary>
    public UserId PublisherId { get; private set; } = default!;
    /// <summary>
    /// 发布人姓名（冗余，便于展示）
    /// </summary>
    public string PublisherName { get; private set; } = string.Empty;
    /// <summary>
    /// 公告状态：草稿或已发布
    /// </summary>
    public AnnouncementStatus Status { get; private set; }
    /// <summary>
    /// 发布时间；草稿时为 null
    /// </summary>
    public DateTimeOffset? PublishAt { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建一条新公告（初始为草稿状态）
    /// </summary>
    /// <param name="publisherId">发布人用户ID</param>
    /// <param name="publisherName">发布人姓名</param>
    /// <param name="title">公告标题</param>
    /// <param name="content">公告正文</param>
    public Announcement(UserId publisherId, string publisherName, string title, string content)
    {
        PublisherId = publisherId;
        PublisherName = publisherName ?? string.Empty;
        Title = title ?? string.Empty;
        Content = content ?? string.Empty;
        Status = AnnouncementStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 修改草稿的标题与内容；仅当状态为草稿时可调用
    /// </summary>
    /// <param name="title">新标题</param>
    /// <param name="content">新正文</param>
    /// <exception cref="KnownException">当前不是草稿状态时抛出</exception>
    public void UpdateDraft(string title, string content)
    {
        if (Status != AnnouncementStatus.Draft)
            throw new KnownException("只有草稿可修改", ErrorCodes.AnnouncementNotDraft);
        Title = title ?? string.Empty;
        Content = content ?? string.Empty;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 发布公告；仅当状态为草稿时可调用，发布后记录发布时间
    /// </summary>
    /// <exception cref="KnownException">当前不是草稿状态时抛出</exception>
    public void Publish()
    {
        if (Status != AnnouncementStatus.Draft)
            throw new KnownException("只有草稿可发布", ErrorCodes.AnnouncementNotDraft);
        Status = AnnouncementStatus.Published;
        PublishAt = DateTimeOffset.UtcNow;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
