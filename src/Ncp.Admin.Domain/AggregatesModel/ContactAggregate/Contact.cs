using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ContactAggregate;

/// <summary>
/// 联系人ID（强类型ID）
/// </summary>
public partial record ContactId : IGuidStronglyTypedId;

/// <summary>
/// 联系人聚合根，企业通讯录/外部联系人
/// </summary>
public class Contact : Entity<ContactId>, IAggregateRoot
{
    protected Contact() { }

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 电话
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// 公司/部门（可选）
    /// </summary>
    public string? Company { get; private set; }

    /// <summary>
    /// 所属分组ID（可选）
    /// </summary>
    public ContactGroupId? GroupId { get; private set; }

    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建联系人
    /// </summary>
    public Contact(string name, UserId creatorId, string? phone = null, string? email = null, string? company = null, ContactGroupId? groupId = null)
    {
        Name = name ?? string.Empty;
        CreatorId = creatorId;
        Phone = phone;
        Email = email;
        Company = company;
        GroupId = groupId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新联系人信息
    /// </summary>
    public void Update(string name, string? phone, string? email, string? company, ContactGroupId? groupId)
    {
        Name = name ?? string.Empty;
        Phone = phone;
        Email = email;
        Company = company;
        GroupId = groupId;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
