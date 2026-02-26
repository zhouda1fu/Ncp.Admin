using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户联系人 ID（强类型）
/// </summary>
public partial record CustomerContactId : IGuidStronglyTypedId;

/// <summary>
/// 客户联系人子实体，通过 <see cref="Customer"/> 的 AddContact/UpdateContact/RemoveContact 维护
/// </summary>
public class CustomerContact : Entity<CustomerContactId>
{
    /// <summary>
    /// EF/序列化用
    /// </summary>
    protected CustomerContact() { }

    /// <summary>
    /// 所属客户 ID
    /// </summary>
    public CustomerId CustomerId { get; private set; } = default!;

    /// <summary>
    /// 联系人姓名
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 联系人类型（字典）
    /// </summary>
    public string ContactType { get; private set; } = string.Empty;

    /// <summary>
    /// 性别（字典）
    /// </summary>
    public int? Gender { get; private set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateTime? Birthday { get; private set; }

    /// <summary>
    /// 职位
    /// </summary>
    public string Position { get; private set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    public string Mobile { get; private set; } = string.Empty;

    /// <summary>
    /// 座机
    /// </summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// 是否主联系人
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// 创建联系人（由聚合根调用）
    /// </summary>
    internal static CustomerContact Create(
        CustomerId customerId,
        string name,
        string contactType,
        int? gender,
        DateTime? birthday,
        string position,
        string mobile,
        string phone,
        string email,
        bool isPrimary)
    {
        return new CustomerContact
        {
            CustomerId = customerId,
            Name = name ,
            ContactType = contactType ,
            Gender = gender,
            Birthday = birthday,
            Position = position ,
            Mobile = mobile ,
            Phone = phone ,
            Email = email ,
            IsPrimary = isPrimary,
        };
    }

    /// <summary>
    /// 更新联系人信息（由聚合根调用）
    /// </summary>
    internal void Update(
        string name,
        string contactType,
        int? gender,
        DateTime? birthday,
        string position,
        string mobile,
        string phone,
        string email,
        bool isPrimary)
    {
        Name = name ;
        ContactType = contactType ;
        Gender = gender;
        Birthday = birthday;
        Position = position ;
        Mobile = mobile ;
        Phone = phone ;
        Email = email ;
        IsPrimary = isPrimary;
    }
}
