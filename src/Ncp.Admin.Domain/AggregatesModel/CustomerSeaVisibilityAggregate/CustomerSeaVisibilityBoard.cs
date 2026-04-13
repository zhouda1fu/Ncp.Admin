using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerSeaVisibilityAggregate;

/// <summary>
/// 公海客户片区可见性条目 ID
/// </summary>
public partial record CustomerSeaVisibilityEntryId : IGuidStronglyTypedId;

/// <summary>
/// 某公海客户对指定用户的可见性授权条目（可撤销后再次生效）。
/// </summary>
public class CustomerSeaVisibilityEntry : Entity<CustomerSeaVisibilityEntryId>
{
    protected CustomerSeaVisibilityEntry()
    {
    }

    public CustomerSeaVisibilityEntry(CustomerId boardId, UserId userId, DateTimeOffset grantedAt)
    {
        BoardId = boardId;
        UserId = userId;
        GrantedAt = grantedAt;
        RevokedAt = null;
    }

    /// <summary>
    /// 所属可见性板（与客户 ID 一致）
    /// </summary>
    public CustomerId BoardId { get; private set; } = default!;

    public UserId UserId { get; private set; } = default!;

    public DateTimeOffset GrantedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public void Revoke(DateTimeOffset at) => RevokedAt = at;

    public void Restore(DateTimeOffset at)
    {
        RevokedAt = null;
        GrantedAt = at;
    }
}

/// <summary>
/// 同步可见性授权后的差异结果（用于发送通知）。
/// </summary>
public record CustomerSeaVisibilitySyncResult(
    IReadOnlyList<UserId> GrantedUserIds,
    IReadOnlyList<UserId> RevokedUserIds);

/// <summary>
/// 客户公海片区可见性：按客户 ID 作为聚合根主键，与 Customer 一对一。
/// </summary>
public class CustomerSeaVisibilityBoard : Entity<CustomerId>, IAggregateRoot
{
    protected CustomerSeaVisibilityBoard()
    {
    }

    public CustomerSeaVisibilityBoard(CustomerId customerId)
    {
        Id = customerId;
    }

    public virtual ICollection<CustomerSeaVisibilityEntry> Entries { get; } = [];

    /// <summary>
    /// 将当前生效用户集合同步为 desired；返回新增授权与撤回的用户列表。
    /// </summary>
    public CustomerSeaVisibilitySyncResult SyncToDesiredUsers(IEnumerable<UserId> desiredUserIds, DateTimeOffset now)
    {
        var desired = desiredUserIds.Distinct().ToHashSet();
        var granted = new List<UserId>();
        var revoked = new List<UserId>();

        foreach (var entry in Entries.Where(e => e.RevokedAt == null).ToList())
        {
            if (!desired.Contains(entry.UserId))
            {
                entry.Revoke(now);
                revoked.Add(entry.UserId);
            }
        }

        foreach (var uid in desired)
        {
            var existing = Entries.FirstOrDefault(e => e.UserId == uid);
            if (existing is null)
            {
                Entries.Add(new CustomerSeaVisibilityEntry(Id, uid, now));
                granted.Add(uid);
            }
            else if (existing.RevokedAt is not null)
            {
                existing.Restore(now);
                granted.Add(uid);
            }
        }

        return new CustomerSeaVisibilitySyncResult(granted, revoked);
    }
}
