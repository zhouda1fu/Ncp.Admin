using System;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

/// <summary>
/// 项目联系人 ID（强类型）
/// </summary>
public partial record ProjectContactId : IGuidStronglyTypedId;

/// <summary>
/// 项目联系人子实体，通过 <see cref="Project"/> 的 AddContact/UpdateContact/RemoveContact 维护
/// </summary>
public class ProjectContact : Entity<ProjectContactId>
{
    /// <summary>未关联客户联系人时的占位 ID（<see cref="Guid.Empty"/>）</summary>
    public static CustomerContactId NoLinkedCustomerContactId { get; } = new(Guid.Empty);

    protected ProjectContact() { }

    /// <summary>
    /// 所属项目 ID
    /// </summary>
    public ProjectId ProjectId { get; private set; } = default!;

    /// <summary>
    /// 关联的客户联系人 ID（未关联为 <see cref="NoLinkedCustomerContactId"/>）
    /// </summary>
    public CustomerContactId CustomerContactId { get; private set; } = NoLinkedCustomerContactId;

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 职位
    /// </summary>
    public string Position { get; private set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    public string Mobile { get; private set; } = string.Empty;

    /// <summary>
    /// 办公电话
    /// </summary>
    public string OfficePhone { get; private set; } = string.Empty;

    /// <summary>
    /// QQ
    /// </summary>
    public string QQ { get; private set; } = string.Empty;

    /// <summary>
    /// 微信
    /// </summary>
    public string Wechat { get; private set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// 是否主要联系人
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    internal ProjectContact(
        ProjectId projectId,
        CustomerContactId? customerContactId,
        string name,
        string position,
        string mobile,
        string officePhone,
        string qq,
        string wechat,
        string email,
        bool isPrimary,
        string remark)
    {
        ProjectId = projectId;
        CustomerContactId = customerContactId ?? NoLinkedCustomerContactId;
        Name = name ?? string.Empty;
        Position = position ?? string.Empty;
        Mobile = mobile ?? string.Empty;
        OfficePhone = officePhone ?? string.Empty;
        QQ = qq ?? string.Empty;
        Wechat = wechat ?? string.Empty;
        Email = email ?? string.Empty;
        IsPrimary = isPrimary;
        Remark = remark ?? string.Empty;
    }

    internal void Update(
        CustomerContactId? customerContactId,
        string name,
        string position,
        string mobile,
        string officePhone,
        string qq,
        string wechat,
        string email,
        bool isPrimary,
        string remark)
    {
        CustomerContactId = customerContactId ?? NoLinkedCustomerContactId;
        Name = name ?? string.Empty;
        Position = position ?? string.Empty;
        Mobile = mobile ?? string.Empty;
        OfficePhone = officePhone ?? string.Empty;
        QQ = qq ?? string.Empty;
        Wechat = wechat ?? string.Empty;
        Email = email ?? string.Empty;
        IsPrimary = isPrimary;
        Remark = remark ?? string.Empty;
    }
}
