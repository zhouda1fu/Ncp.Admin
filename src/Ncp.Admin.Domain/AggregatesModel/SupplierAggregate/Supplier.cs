using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;

/// <summary>
/// 供应商 ID（强类型）
/// </summary>
public partial record SupplierId : IGuidStronglyTypedId;

/// <summary>
/// 供应商聚合根：供产品关联
/// </summary>
public class Supplier : Entity<SupplierId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected Supplier() { }

    /// <summary>供应商全称</summary>
    public string FullName { get; private set; } = default!;

    /// <summary>简称</summary>
    public string ShortName { get; private set; } = default!;

    /// <summary>联系人</summary>
    public string Contact { get; private set; } = default!;

    /// <summary>电话</summary>
    public string Phone { get; private set; } = default!;

    /// <summary>邮箱</summary>
    public string Email { get; private set; } = default!;

    /// <summary>地址</summary>
    public string Address { get; private set; } = default!;

    /// <summary>备注</summary>
    public string Remark { get; private set; } = default!;

    /// <summary>
    /// 创建供应商
    /// </summary>
    public Supplier(string fullName, string shortName, string contact, string phone, string email, string address, string remark)
    {
        FullName = fullName;
        ShortName = shortName;
        Contact = contact;
        Phone = phone;
        Email = email;
        Address = address;
        Remark = remark;
    }

    /// <summary>
    /// 更新供应商
    /// </summary>
    public void Update(string fullName, string shortName, string contact, string phone, string email, string address, string remark)
    {
        FullName = fullName;
        ShortName = shortName;
        Contact = contact;
        Phone = phone;
        Email = email;
        Address = address;
        Remark = remark;
    }
}
