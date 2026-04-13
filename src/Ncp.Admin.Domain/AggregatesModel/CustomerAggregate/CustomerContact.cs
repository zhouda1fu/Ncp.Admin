using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户联系人 ID（强类型）
/// </summary>
public partial record CustomerContactId : IGuidStronglyTypedId;

/// <summary>
/// 客户联系人子实体，仅通过 <see cref="Customer"/> 的领域行为挂入 <see cref="Customer.Contacts"/>，<see cref="CustomerId"/> 由 EF 根据父子关系修复。
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
    public int Gender { get; private set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateTimeOffset Birthday { get; private set; }

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
    /// QQ
    /// </summary>
    public string Qq { get; private set; } = string.Empty;

    /// <summary>
    /// 微信
    /// </summary>
    public string Wechat { get; private set; } = string.Empty;

    /// <summary>
    /// 微信添加情况
    /// </summary>
    public bool IsWechatAdded { get; private set; }

    /// <summary>
    /// 是否主联系人
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// 挂入 <see cref="Customer.Contacts"/>：不填 <see cref="CustomerId"/>，由 EF 在持久化前修复。
    /// </summary>
    internal CustomerContact(
        string name,
        string contactType,
        int gender,
        DateTimeOffset birthday,
        string position,
        string mobile,
        string phone,
        string email,
        string qq,
        string wechat,
        bool isWechatAdded,
        bool isPrimary)
    {
        Name = name;
        ContactType = contactType;
        Gender = gender;
        Birthday = birthday;
        Position = position;
        Mobile = mobile;
        Phone = phone;
        Email = email;
        Qq = qq;
        Wechat = wechat;
        IsWechatAdded = isWechatAdded;
        IsPrimary = isPrimary;
    }

    /// <summary>
    /// 更新联系人信息（由聚合根调用）
    /// </summary>
    internal void Update(
        string name,
        string contactType,
        int gender,
        DateTimeOffset birthday,
        string position,
        string mobile,
        string phone,
        string email,
        string qq,
        string wechat,
        bool isWechatAdded,
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
        Qq = qq;
        Wechat = wechat;
        IsWechatAdded = isWechatAdded;
        IsPrimary = isPrimary;
    }
}
