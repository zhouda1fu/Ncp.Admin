using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;

/// <summary>
/// 聊天组成员ID（强类型ID）
/// </summary>
public partial record ChatGroupMemberId : IGuidStronglyTypedId;

/// <summary>
/// 聊天组成员（聚合内实体）
/// </summary>
public class ChatGroupMember : Entity<ChatGroupMemberId>
{
    protected ChatGroupMember() { }

    /// <summary>
    /// 用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// 加入时间
    /// </summary>
    public DateTimeOffset JoinedAt { get; init; }

    internal ChatGroupMember(UserId userId)
    {
        UserId = userId;
        JoinedAt = DateTimeOffset.UtcNow;
    }
}
