using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents.CustomerEvents;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户 ID（强类型）
/// </summary>
public partial record CustomerId : IGuidStronglyTypedId;

/// <summary>
/// 客户聚合根：档案、联系人、行业、公海归属等
/// </summary>
public class Customer : Entity<CustomerId>, IAggregateRoot
{
    /// <summary>
    /// EF/序列化用
    /// </summary>
    protected Customer() { }

    /// <summary>
    /// 联系人列表
    /// </summary>
    public virtual ICollection<CustomerContact> Contacts { get; } = [];

    /// <summary>
    /// 所属行业关联列表
    /// </summary>
    public virtual ICollection<CustomerIndustry> Industries { get; } = [];

    /// <summary>
    /// 负责人用户 ID
    /// </summary>
    public UserId? OwnerId { get; private set; }

    /// <summary>
    /// 所属部门 ID
    /// </summary>
    public DeptId? DeptId { get; private set; }

    /// <summary>
    /// 客户来源 ID（主数据，必填）
    /// </summary>
    public CustomerSourceId CustomerSourceId { get; private set; } = default!;

    /// <summary>
    /// 客户来源名称（冗余，便于列表/展示）
    /// </summary>
    public string CustomerSourceName { get; private set; } = string.Empty;

    /// <summary>
    /// 客户状态（字典）
    /// </summary>
    public int StatusId { get; private set; }

    /// <summary>
    /// 客户全称
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// 客户简称
    /// </summary>
    public string ShortName { get; private set; } = string.Empty;

    /// <summary>
    /// 公司性质
    /// </summary>
    public string Nature { get; private set; } = string.Empty;

    /// <summary>
    /// 省区域码
    /// </summary>
    public string ProvinceCode { get; private set; } = string.Empty;

    /// <summary>
    /// 市区域码
    /// </summary>
    public string CityCode { get; private set; } = string.Empty;

    /// <summary>
    /// 区/县区域码
    /// </summary>
    public string DistrictCode { get; private set; } = string.Empty;

    /// <summary>
    /// 省名称（冗余，便于列表/展示）
    /// </summary>
    public string ProvinceName { get; private set; } = string.Empty;

    /// <summary>
    /// 市名称（冗余，便于列表/展示）
    /// </summary>
    public string CityName { get; private set; } = string.Empty;

    /// <summary>
    /// 区/县名称（冗余，便于列表/展示）
    /// </summary>
    public string DistrictName { get; private set; } = string.Empty;

    /// <summary>
    /// 客户覆盖区域
    /// </summary>
    public string CoverRegion { get; private set; } = string.Empty;

    /// <summary>
    /// 公司注册地址
    /// </summary>
    public string RegisterAddress { get; private set; } = string.Empty;

    /// <summary>
    /// 主联系人姓名
    /// </summary>
    public string MainContactName { get; private set; } = string.Empty;

    /// <summary>
    /// 主联系方式
    /// </summary>
    public string MainContactPhone { get; private set; } = string.Empty;

    /// <summary>
    /// 微信添加情况
    /// </summary>
    public string WechatStatus { get; private set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>
    /// 是否大客户
    /// </summary>
    public bool IsKeyAccount { get; private set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// 合并标识
    /// </summary>
    public bool CombineFlag { get; private set; }

    /// <summary>
    /// 是否在公海
    /// </summary>
    public bool IsInSea { get; private set; }

    /// <summary>
    /// 释放到公海时间
    /// </summary>
    public DateTimeOffset? ReleasedToSeaAt { get; private set; }

    /// <summary>
    /// 创建人用户 ID
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
    /// 创建客户；未传负责人则视为公海客户
    /// </summary>
    public Customer(
        UserId? ownerId,
        DeptId? deptId,
        CustomerSourceId customerSourceId,
        string customerSourceName,
        int statusId,
        string fullName,
        string shortName,
        string nature,
        string provinceCode,
        string cityCode,
        string districtCode,
        string provinceName,
        string cityName,
        string districtName,
        string coverRegion,
        string registerAddress,
        string mainContactName,
        string mainContactPhone,
        string wechatStatus,
        string remark,
        bool isKeyAccount,
        UserId creatorId)
    {
        OwnerId = ownerId;
        DeptId = deptId;
        CustomerSourceId = customerSourceId;
        CustomerSourceName = customerSourceName ?? string.Empty;
        StatusId = statusId;
        FullName = fullName;
        ShortName = shortName;
        Nature = nature;
        ProvinceCode = provinceCode;
        CityCode = cityCode;
        DistrictCode = districtCode;
        ProvinceName = provinceName ?? string.Empty;
        CityName = cityName ?? string.Empty;
        DistrictName = districtName ?? string.Empty;
        CoverRegion = coverRegion;
        RegisterAddress = registerAddress;
        MainContactName = mainContactName;
        MainContactPhone = mainContactPhone;
        WechatStatus = wechatStatus;
        Remark = remark;
        IsKeyAccount = isKeyAccount;
        CreatorId = creatorId;
        IsInSea = ownerId == null;
        CreatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CustomerCreatedDomainEvent(this));
    }

    /// <summary>
    /// 设置所属行业（创建后由命令处理器调用）
    /// </summary>
    public void SetIndustries(IEnumerable<IndustryId> industryIds)
    {
        Industries.Clear();
        foreach (var id in industryIds)
            Industries.Add(CustomerIndustry.Create(Id, id));
    }

    /// <summary>
    /// 更新客户档案；公海客户需先领用后再改
    /// </summary>
    public void Update(
        UserId? ownerId,
        DeptId? deptId,
        CustomerSourceId customerSourceId,
        string customerSourceName,
        int statusId,
        string fullName,
        string shortName,
        string nature,
        string provinceCode,
        string cityCode,
        string districtCode,
        string provinceName,
        string cityName,
        string districtName,
        string coverRegion,
        string registerAddress,
        string mainContactName,
        string mainContactPhone,
        string wechatStatus,
        string remark,
        bool isKeyAccount,
        bool isHidden,
        IEnumerable<IndustryId>? industryIds = null)
    {
        if (IsInSea)
            throw new KnownException("公海客户需先领用后再修改", ErrorCodes.CustomerNotInSea);
        OwnerId = ownerId;
        DeptId = deptId;
        CustomerSourceId = customerSourceId;
        CustomerSourceName = customerSourceName ?? string.Empty;
        StatusId = statusId;
        FullName = fullName;
        ShortName = shortName;
        Nature = nature;
        ProvinceCode = provinceCode;
        CityCode = cityCode;
        DistrictCode = districtCode;
        ProvinceName = provinceName ?? string.Empty;
        CityName = cityName ?? string.Empty;
        DistrictName = districtName ?? string.Empty;
        CoverRegion = coverRegion;
        RegisterAddress = registerAddress;
        MainContactName = mainContactName;
        MainContactPhone = mainContactPhone;
        WechatStatus = wechatStatus;
        Remark = remark;
        IsKeyAccount = isKeyAccount;
        IsHidden = isHidden;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        if (industryIds != null)
        {
            Industries.Clear();
            foreach (var id in industryIds)
                Industries.Add(CustomerIndustry.Create(Id, id));
        }
        AddDomainEvent(new CustomerUpdatedDomainEvent(this));
    }

    /// <summary>
    /// 释放到公海
    /// </summary>
    public void ReleaseToSea()
    {
        if (IsInSea)
            return;
        OwnerId = null;
        DeptId = null;
        IsInSea = true;
        ReleasedToSeaAt = DateTimeOffset.UtcNow;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new CustomerReleasedToSeaDomainEvent(this));
    }

    /// <summary>
    /// 从公海领用（仅公海客户可领用）
    /// </summary>
    public void ClaimFromSea(UserId ownerId, DeptId? deptId)
    {
        if (!IsInSea)
            throw new KnownException("仅公海客户可领用", ErrorCodes.CustomerNotInSea);
        OwnerId = ownerId;
        DeptId = deptId;
        IsInSea = false;
        ReleasedToSeaAt = null;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new CustomerClaimedFromSeaDomainEvent(this));
    }

    /// <summary>
    /// 添加联系人
    /// </summary>
    public CustomerContactId AddContact(
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
        var contact = CustomerContact.Create(Id, name, contactType, gender, birthday, position, mobile, phone, email, isPrimary);
        Contacts.Add(contact);
        if (isPrimary)
        {
            foreach (var c in Contacts.Where(c => c != contact))
                c.Update(c.Name, c.ContactType, c.Gender, c.Birthday, c.Position, c.Mobile, c.Phone, c.Email, false);
        }
        AddDomainEvent(new CustomerContactAddedDomainEvent(this, contact));
        return contact.Id;
    }

    /// <summary>
    /// 更新指定联系人
    /// </summary>
    public void UpdateContact(
        CustomerContactId contactId,
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
        var contact = Contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new KnownException("未找到客户联系人", ErrorCodes.CustomerContactNotFound);
        contact.Update(name, contactType, gender, birthday, position, mobile, phone, email, isPrimary);
        if (isPrimary)
        {
            foreach (var c in Contacts.Where(c => c != contact))
                c.Update(c.Name, c.ContactType, c.Gender, c.Birthday, c.Position, c.Mobile, c.Phone, c.Email, false);
        }
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new CustomerContactUpdatedDomainEvent(this, contact));
    }

    /// <summary>
    /// 移除指定联系人
    /// </summary>
    public void RemoveContact(CustomerContactId contactId)
    {
        var contact = Contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new KnownException("未找到客户联系人", ErrorCodes.CustomerContactNotFound);
        AddDomainEvent(new CustomerContactRemovedDomainEvent(this, contact.Id));
        Contacts.Remove(contact);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
