using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;

/// <summary>
/// 联系组ID（强类型ID）
/// </summary>
public partial record ContactGroupId : IGuidStronglyTypedId;

/// <summary>
/// 联系组聚合根，用于通讯录分组（如：客户、供应商、同事等）
/// </summary>
public class ContactGroup : Entity<ContactGroupId>, IAggregateRoot
{
    protected ContactGroup() { }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;

    /// <summary>
    /// 排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建联系组
    /// </summary>
    public ContactGroup(string name, UserId creatorId, int sortOrder = 0)
    {
        Name = name ?? string.Empty;
        CreatorId = creatorId;
        SortOrder = sortOrder;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新分组信息
    /// </summary>
    public void Update(string name, int sortOrder)
    {
        Name = name ?? string.Empty;
        SortOrder = sortOrder;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
