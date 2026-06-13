using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;

public partial record UserCalendarMemoId : IInt64StronglyTypedId
{
    public static UserCalendarMemoId Unassigned { get; } = new(0);
}

/// <summary>用户行事历便签（每用户每自然日至多一条）。</summary>
public class UserCalendarMemo : Entity<UserCalendarMemoId>, IAggregateRoot
{
    protected UserCalendarMemo()
    {
    }

    public UserCalendarMemo(UserId userId, DateOnly memoDate, string content)
    {
        UserId = userId;
        MemoDate = memoDate;
        Content = content;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public UserId UserId { get; private set; } = UserId.Unassigned;

    public DateOnly MemoDate { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
