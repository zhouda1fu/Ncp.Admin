using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;

/// <summary>
/// 聊天组ID（强类型ID）
/// </summary>
public partial record ChatGroupId : IGuidStronglyTypedId;

/// <summary>
/// 聊天类型：单聊、群聊
/// </summary>
public enum ChatGroupType
{
    /// <summary>
    /// 单聊（1:1）
    /// </summary>
    Single = 0,
    /// <summary>
    /// 群聊
    /// </summary>
    Group = 1,
}

/// <summary>
/// 聊天组聚合根（单聊或群聊会话）
/// </summary>
public class ChatGroup : Entity<ChatGroupId>, IAggregateRoot
{
    private readonly List<ChatGroupMember> _members = [];

    protected ChatGroup() { }

    /// <summary>
    /// 群名称（单聊可为空）
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// 聊天类型
    /// </summary>
    public ChatGroupType Type { get; private set; }

    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 成员列表（只读）
    /// </summary>
    public IReadOnlyList<ChatGroupMember> Members => _members.AsReadOnly();

    /// <summary>
    /// 创建单聊（两人）
    /// </summary>
    public ChatGroup(UserId creatorId, UserId otherUserId)
    {
        Type = ChatGroupType.Single;
        CreatorId = creatorId;
        CreatedAt = DateTimeOffset.UtcNow;
        _members.Add(new ChatGroupMember(creatorId));
        _members.Add(new ChatGroupMember(otherUserId));
    }

    /// <summary>
    /// 创建群聊
    /// </summary>
    public ChatGroup(string name, UserId creatorId, IEnumerable<UserId> memberIds)
    {
        Name = name ;
        Type = ChatGroupType.Group;
        CreatorId = creatorId;
        CreatedAt = DateTimeOffset.UtcNow;
        _members.Add(new ChatGroupMember(creatorId));
        foreach (var id in memberIds ?? [])
        {
            if (id != creatorId && _members.All(m => m.UserId != id))
                _members.Add(new ChatGroupMember(id));
        }
    }

    /// <summary>
    /// 添加成员（仅群聊）
    /// </summary>
    public void AddMember(UserId userId)
    {
        if (Type != ChatGroupType.Group) return;
        if (_members.Any(m => m.UserId == userId)) return;
        _members.Add(new ChatGroupMember(userId));
    }

    /// <summary>
    /// 移除成员（仅群聊）
    /// </summary>
    public void RemoveMember(UserId userId)
    {
        if (Type != ChatGroupType.Group) return;
        if (userId == CreatorId) return;
        var m = _members.FirstOrDefault(x => x.UserId == userId);
        if (m != null) _members.Remove(m);
    }

    /// <summary>
    /// 是否包含某用户
    /// </summary>
    public bool ContainsUser(UserId userId) => _members.Any(m => m.UserId == userId);
}
