using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

/// <summary>
/// 项目跟进记录 ID（强类型）
/// </summary>
public partial record ProjectFollowUpRecordId : IGuidStronglyTypedId;

/// <summary>
/// 项目跟进记录子实体，通过 <see cref="Project"/> 的 AddFollowUpRecord/UpdateFollowUpRecord/RemoveFollowUpRecord 维护
/// </summary>
public class ProjectFollowUpRecord : Entity<ProjectFollowUpRecordId>
{
    protected ProjectFollowUpRecord() { }

    /// <summary>
    /// 所属项目 ID
    /// </summary>
    public ProjectId ProjectId { get; private set; } = default!;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// 拜访日期
    /// </summary>
    public DateOnly? VisitDate { get; private set; }

    /// <summary>
    /// 提醒频率（每 N 天提醒，0 表示不提醒）
    /// </summary>
    public int ReminderIntervalDays { get; private set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 创建人用户 ID（系统/未知为 <c>new UserId(0)</c>）
    /// </summary>
    public UserId CreatorId { get; private set; } = new UserId(0);

    internal ProjectFollowUpRecord(
        ProjectId projectId,
        string title,
        DateOnly? visitDate,
        int reminderIntervalDays,
        string content,
        UserId? creatorId)
    {
        ProjectId = projectId;
        Title = title ?? string.Empty;
        VisitDate = visitDate;
        ReminderIntervalDays = reminderIntervalDays >= 0 ? reminderIntervalDays : 0;
        Content = content ?? string.Empty;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatorId = creatorId ?? new UserId(0);
    }

    internal void Update(string title, DateOnly? visitDate, int reminderIntervalDays, string content)
    {
        Title = title ?? string.Empty;
        VisitDate = visitDate;
        ReminderIntervalDays = reminderIntervalDays >= 0 ? reminderIntervalDays : 0;
        Content = content ?? string.Empty;
    }
}
