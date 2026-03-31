using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// 用户岗位关系实体
/// 表示用户与岗位的一对一关系
/// </summary>
public class UserPosition
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// 岗位ID
    /// </summary>
    public PositionId PositionId { get; private set; } = default!;

    /// <summary>
    /// 岗位名称（冗余存储，岗位名称变更时通过领域事件同步）
    /// </summary>
    public string PositionName { get; private set; } = string.Empty;

    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTimeOffset AssignedAt { get; init; }

    protected UserPosition()
    {
    }

    /// <summary>
    /// 创建用户岗位关系
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="positionId">岗位ID</param>
    /// <param name="positionName">岗位名称</param>
    public UserPosition(UserId userId, PositionId positionId, string positionName)
    {
        UserId = userId;
        PositionId = positionId;
        AssignedAt = DateTimeOffset.UtcNow;
        PositionName = positionName ?? string.Empty;
    }

    /// <summary>
    /// 更新岗位名称
    /// </summary>
    /// <param name="positionName">新的岗位名称</param>
    public void UpdatePositionName(string positionName)
    {
        if (string.IsNullOrWhiteSpace(positionName))
        {
            return;
        }

        PositionName = positionName;
    }
}
