using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;

/// <summary>
/// 客户公海片区分配 聚合根 ID
/// </summary>
public partial record CustomerSeaRegionAssignmentId : IGuidStronglyTypedId;

/// <summary>
/// 客户公海片区分配：为某个用户维护其绑定的地区集合。
/// </summary>
public class CustomerSeaRegionAssignment : Entity<CustomerSeaRegionAssignmentId>, IAggregateRoot
{
    protected CustomerSeaRegionAssignment() { }

    /// <summary>
    /// 被分配对象（用户）
    /// </summary>
    public UserId TargetUserId { get; private set; } = default!;

    /// <summary>
    /// 绑定的地区集合（最终保存的展开结果）
    /// </summary>
    public virtual ICollection<CustomerSeaRegionAssignmentRegion> Regions { get; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ModifiedAt { get; private set; }

    /// <summary>新建分配（业务；EF 使用无参 <see cref="CustomerSeaRegionAssignment" />）</summary>
    public CustomerSeaRegionAssignment(UserId targetUserId)
    {
        var now = DateTimeOffset.UtcNow;
        TargetUserId = targetUserId;
        CreatedAt = now;
        ModifiedAt = now;
    }

    /// <summary>
    /// 以差集方式更新绑定地区集合，避免 Clear + 全量 Add 导致复合主键冲突/状态混乱。
    /// </summary>
    public void UpdateRegionIds(IEnumerable<RegionId> expandedRegionIds)
    {
        ModifiedAt = DateTimeOffset.UtcNow;

        var desired = (expandedRegionIds ?? []).Distinct().ToList();
        var desiredSet = desired.ToHashSet();

        foreach (var link in Regions.Where(r => !desiredSet.Contains(r.RegionId)).ToList())
            Regions.Remove(link);

        var existing = Regions.Select(r => r.RegionId).ToHashSet();
        foreach (var regionId in desired.Where(id => !existing.Contains(id)))
            Regions.Add(new CustomerSeaRegionAssignmentRegion(Id, regionId));
    }
}

/// <summary>
/// 客户公海片区分配 - 地区绑定（一个用户可绑定多个地区）。
/// </summary>
public class CustomerSeaRegionAssignmentRegion
{
    protected CustomerSeaRegionAssignmentRegion() { }

    public CustomerSeaRegionAssignmentRegion(CustomerSeaRegionAssignmentId assignmentId, RegionId regionId)
    {
        AssignmentId = assignmentId;
        RegionId = regionId;
    }

    public CustomerSeaRegionAssignmentId AssignmentId { get; private set; } = default!;

    public RegionId RegionId { get; private set; } = default!;
}

