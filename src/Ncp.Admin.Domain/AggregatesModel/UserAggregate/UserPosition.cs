using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// 用户岗位关系实体（与用户一对一）
/// </summary>
public class UserPosition : Entity<UserId>
{
    /// <summary>
    /// EF/序列化用
    /// </summary>
    protected UserPosition()
    {
    }

    /// <summary>
    /// 岗位ID
    /// </summary>
    public PositionId PositionId { get; private set; } = PositionId.Unassigned;

    /// <summary>
    /// 岗位名称（冗余存储，岗位名称变更时通过领域事件同步）
    /// </summary>
    public string PositionName { get; private set; } = string.Empty;

    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTimeOffset AssignedAt { get; init; }

    /// <summary>
    /// 创建用户岗位关系
    /// </summary>
    /// <param name="positionId">岗位ID</param>
    /// <param name="positionName">岗位名称</param>
    internal UserPosition(PositionId positionId, string positionName)
    {
        PositionId = positionId;
        AssignedAt = DateTimeOffset.UtcNow;
        PositionName = positionName;
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
