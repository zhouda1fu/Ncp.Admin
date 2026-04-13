using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;

/// <summary>
/// 审计变更类型：新增/删除地区。
/// </summary>
public enum CustomerSeaRegionAssignmentAuditChangeType
{
    Added = 0,
    Removed = 1,
}

/// <summary>
/// 客户公海片区分配审计 ID
/// </summary>
public partial record CustomerSeaRegionAssignmentAuditId : IGuidStronglyTypedId;

/// <summary>
/// 客户公海片区分配审计聚合根（仅追加，不修改）
/// </summary>
public class CustomerSeaRegionAssignmentAudit : Entity<CustomerSeaRegionAssignmentAuditId>, IAggregateRoot
{
    protected CustomerSeaRegionAssignmentAudit() { }

    /// <summary>
    /// 被修改对象（用户）
    /// </summary>
    public UserId TargetUserId { get; private set; } = default!;

    /// <summary>
    /// 操作人（用户）
    /// </summary>
    public UserId OperatorUserId { get; private set; } = default!;

    /// <summary>
    /// 操作人姓名（冗余，便于展示）
    /// </summary>
    public string OperatorUserName { get; private set; } = string.Empty;

    /// <summary>
    /// 操作时间（UTC）
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// 明细（新增/删除地区快照）
    /// </summary>
    public virtual ICollection<CustomerSeaRegionAssignmentAuditDetail> Details { get; } = [];

    /// <summary>新建审计记录（仅追加；EF 使用无参 <see cref="CustomerSeaRegionAssignmentAudit" />）</summary>
    public CustomerSeaRegionAssignmentAudit(
        UserId targetUserId,
        UserId operatorUserId,
        string operatorUserName,
        IEnumerable<(RegionId RegionId, string RegionName, CustomerSeaRegionAssignmentAuditChangeType ChangeType)> details)
    {
        var now = DateTimeOffset.UtcNow;
        TargetUserId = targetUserId;
        OperatorUserId = operatorUserId;
        OperatorUserName = operatorUserName ?? string.Empty;
        CreatedAt = now;

        foreach (var d in details ?? [])
            Details.Add(new CustomerSeaRegionAssignmentAuditDetail(Id, d.RegionId, d.RegionName, d.ChangeType));
    }
}

/// <summary>
/// 客户公海片区分配审计明细
/// </summary>
public class CustomerSeaRegionAssignmentAuditDetail
{
    protected CustomerSeaRegionAssignmentAuditDetail() { }

    public CustomerSeaRegionAssignmentAuditDetail(
        CustomerSeaRegionAssignmentAuditId auditId,
        RegionId regionId,
        string regionNameSnapshot,
        CustomerSeaRegionAssignmentAuditChangeType changeType)
    {
        AuditId = auditId;
        RegionId = regionId;
        RegionNameSnapshot = regionNameSnapshot ?? string.Empty;
        ChangeType = changeType;
    }

    public CustomerSeaRegionAssignmentAuditId AuditId { get; private set; } = default!;

    public RegionId RegionId { get; private set; } = default!;

    public string RegionNameSnapshot { get; private set; } = string.Empty;

    public CustomerSeaRegionAssignmentAuditChangeType ChangeType { get; private set; }
}

