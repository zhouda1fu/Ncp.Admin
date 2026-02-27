using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
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
    /// 客户来源 ID（主数据，必填）
    /// </summary>
    public CustomerSourceId CustomerSourceId { get; private set; } = default!;

    /// <summary>
    /// 客户来源名称（冗余，便于列表/展示）
    /// </summary>
    public string CustomerSourceName { get; private set; } = string.Empty;

    /// <summary>
    /// 是否作废客户
    /// </summary>
    public bool IsVoided { get; private set; }

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
    /// 电话区域-省区域码
    /// </summary>
    public string PhoneProvinceCode { get; private set; } = string.Empty;

    /// <summary>
    /// 电话区域-市区域码
    /// </summary>
    public string PhoneCityCode { get; private set; } = string.Empty;

    /// <summary>
    /// 电话区域-区/县区域码
    /// </summary>
    public string PhoneDistrictCode { get; private set; } = string.Empty;

    /// <summary>
    /// 电话区域-省/市/区名称（冗余）
    /// </summary>
    public string PhoneProvinceName { get; private set; } = string.Empty;

    public string PhoneCityName { get; private set; } = string.Empty;

    public string PhoneDistrictName { get; private set; } = string.Empty;

    /// <summary>
    /// 咨询内容（公海录入）
    /// </summary>
    public string ConsultationContent { get; private set; } = string.Empty;

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
    /// 联系人QQ（公海/线索）
    /// </summary>
    public string ContactQq { get; private set; } = string.Empty;

    /// <summary>
    /// 联系人微信（公海/线索，与 WechatStatus 区分：此处存微信号等）
    /// </summary>
    public string ContactWechat { get; private set; } = string.Empty;

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
    /// 创建人姓名（冗余，便于列表/展示）
    /// </summary>
    public string CreatorName { get; private set; } = string.Empty;

    /// <summary>
    /// 领用人/负责人姓名（冗余，OwnerId 对应姓名，便于列表/展示）
    /// </summary>
    public string OwnerName { get; private set; } = string.Empty;

    /// <summary>
    /// 领用时间（从公海领用时写入，释放回公海时清空）
    /// </summary>
    public DateTimeOffset? ClaimedAt { get; private set; }

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
        CustomerSourceId customerSourceId,
        string customerSourceName,
        string fullName,
        string shortName,
        string nature,
        string provinceCode,
        string cityCode,
        string districtCode,
        string provinceName,
        string cityName,
        string districtName,
        string phoneProvinceCode,
        string phoneCityCode,
        string phoneDistrictCode,
        string phoneProvinceName,
        string phoneCityName,
        string phoneDistrictName,
        string consultationContent,
        string coverRegion,
        string registerAddress,
        string mainContactName,
        string mainContactPhone,
        string contactQq,
        string contactWechat,
        string wechatStatus,
        string remark,
        bool isKeyAccount,
        UserId creatorId,
        string creatorName)
    {
        OwnerId = ownerId;
        CustomerSourceId = customerSourceId;
        CustomerSourceName = customerSourceName;
        IsVoided = false;
        FullName = fullName;
        ShortName = shortName;
        Nature = nature;
        ProvinceCode = provinceCode;
        CityCode = cityCode;
        DistrictCode = districtCode;
        ProvinceName = provinceName;
        CityName = cityName;
        DistrictName = districtName;
        PhoneProvinceCode = phoneProvinceCode;
        PhoneCityCode = phoneCityCode;
        PhoneDistrictCode = phoneDistrictCode;
        PhoneProvinceName = phoneProvinceName;
        PhoneCityName = phoneCityName;
        PhoneDistrictName = phoneDistrictName;
        ConsultationContent = consultationContent;
        CoverRegion = coverRegion;
        RegisterAddress = registerAddress;
        MainContactName = mainContactName;
        MainContactPhone = mainContactPhone;
        ContactQq = contactQq;
        ContactWechat = contactWechat;
        WechatStatus = wechatStatus;
        Remark = remark;
        IsKeyAccount = isKeyAccount;
        CreatorId = creatorId;
        CreatorName = creatorName;
        OwnerName = string.Empty;
        ClaimedAt = null;
        IsInSea = ownerId == null;
        CreatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CustomerCreatedDomainEvent(this));
    }

    /// <summary>
    /// 公海录入创建客户；仅包含公海录入表单字段，无客户名称/负责人/部门，其余属性为默认值。
    /// </summary>
    public Customer(
        CustomerSourceId customerSourceId,
        string customerSourceName,
        string mainContactName,
        string mainContactPhone,
        string contactQq,
        string contactWechat,
        string phoneProvinceCode,
        string phoneCityCode,
        string phoneDistrictCode,
        string phoneProvinceName,
        string phoneCityName,
        string phoneDistrictName,
        string provinceCode,
        string cityCode,
        string districtCode,
        string provinceName,
        string cityName,
        string districtName,
        string consultationContent,
        UserId creatorId,
        string creatorName)
    {
        OwnerId = null;
        CustomerSourceId = customerSourceId;
        CustomerSourceName = customerSourceName;
        IsVoided = false;
        FullName = string.Empty;
        ShortName = string.Empty;
        Nature = string.Empty;
        ProvinceCode = provinceCode;
        CityCode = cityCode;
        DistrictCode = districtCode;
        ProvinceName = provinceName;
        CityName = cityName;
        DistrictName = districtName;
        PhoneProvinceCode = phoneProvinceCode;
        PhoneCityCode = phoneCityCode;
        PhoneDistrictCode = phoneDistrictCode;
        PhoneProvinceName = phoneProvinceName;
        PhoneCityName = phoneCityName;
        PhoneDistrictName = phoneDistrictName;
        ConsultationContent = consultationContent;
        CoverRegion = string.Empty;
        RegisterAddress = string.Empty;
        MainContactName = mainContactName;
        MainContactPhone = mainContactPhone;
        ContactQq = contactQq;
        ContactWechat = contactWechat;
        WechatStatus = string.Empty;
        Remark = string.Empty;
        IsKeyAccount = false;
        CreatorId = creatorId;
        CreatorName = creatorName;
        OwnerName = string.Empty;
        ClaimedAt = null;
        IsInSea = true;
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
        CustomerSourceId customerSourceId,
        string customerSourceName,
        string fullName,
        string shortName,
        string nature,
        string provinceCode,
        string cityCode,
        string districtCode,
        string provinceName,
        string cityName,
        string districtName,
        string phoneProvinceCode,
        string phoneCityCode,
        string phoneDistrictCode,
        string phoneProvinceName,
        string phoneCityName,
        string phoneDistrictName,
        string consultationContent,
        string coverRegion,
        string registerAddress,
        string mainContactName,
        string mainContactPhone,
        string contactQq,
        string contactWechat,
        string wechatStatus,
        string remark,
        bool isKeyAccount,
        bool isHidden,
        IEnumerable<IndustryId>? industryIds = null)
    {
        if (IsInSea)
            throw new KnownException("公海客户需先领用后再修改", ErrorCodes.CustomerNotInSea);
        OwnerId = ownerId;
        CustomerSourceId = customerSourceId;
        CustomerSourceName = customerSourceName;
        FullName = fullName;
        ShortName = shortName;
        Nature = nature;
        ProvinceCode = provinceCode;
        CityCode = cityCode;
        DistrictCode = districtCode;
        ProvinceName = provinceName;
        CityName = cityName;
        DistrictName = districtName;
        PhoneProvinceCode = phoneProvinceCode;
        PhoneCityCode = phoneCityCode;
        PhoneDistrictCode = phoneDistrictCode;
        PhoneProvinceName = phoneProvinceName;
        PhoneCityName = phoneCityName;
        PhoneDistrictName = phoneDistrictName;
        ConsultationContent = consultationContent;
        CoverRegion = coverRegion;
        RegisterAddress = registerAddress;
        MainContactName = mainContactName;
        MainContactPhone = mainContactPhone;
        ContactQq = contactQq;
        ContactWechat = contactWechat;
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
    /// 公海客户档案更新（仅当 IsInSea 时可调用，不变更负责人/部门）
    /// </summary>
    public void UpdateWhenInSea(
        CustomerSourceId customerSourceId,
        string customerSourceName,
        string shortName,
        string nature,
        string provinceCode,
        string cityCode,
        string districtCode,
        string provinceName,
        string cityName,
        string districtName,
        string phoneProvinceCode,
        string phoneCityCode,
        string phoneDistrictCode,
        string phoneProvinceName,
        string phoneCityName,
        string phoneDistrictName,
        string consultationContent,
        string coverRegion,
        string registerAddress,
        string mainContactName,
        string mainContactPhone,
        string contactQq,
        string contactWechat,
        string wechatStatus,
        string remark,
        bool isKeyAccount,
        IEnumerable<IndustryId>? industryIds = null)
    {
        if (!IsInSea)
            throw new KnownException("仅公海客户可在此处修改", ErrorCodes.CustomerNotInSea);
        CustomerSourceId = customerSourceId;
        CustomerSourceName = customerSourceName;
        ShortName = shortName;
        Nature = nature;
        ProvinceCode = provinceCode;
        CityCode = cityCode;
        DistrictCode = districtCode;
        ProvinceName = provinceName;
        CityName = cityName;
        DistrictName = districtName;
        PhoneProvinceCode = phoneProvinceCode;
        PhoneCityCode = phoneCityCode;
        PhoneDistrictCode = phoneDistrictCode;
        PhoneProvinceName = phoneProvinceName;
        PhoneCityName = phoneCityName;
        PhoneDistrictName = phoneDistrictName;
        ConsultationContent = consultationContent;
        CoverRegion = coverRegion;
        RegisterAddress = registerAddress;
        MainContactName = mainContactName;
        MainContactPhone = mainContactPhone;
        ContactQq = contactQq;
        ContactWechat = contactWechat;
        WechatStatus = wechatStatus;
        Remark = remark;
        IsKeyAccount = isKeyAccount;
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
    /// 作废（仅公海客户可作废）
    /// </summary>
    public void Void()
    {
        if (!IsInSea)
            throw new KnownException("仅公海客户可作废", ErrorCodes.CustomerNotInSea);
        IsVoided = true;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 释放到公海
    /// </summary>
    public void ReleaseToSea()
    {
        if (IsInSea)
            return;
        OwnerId = null;
        OwnerName = string.Empty;
        ClaimedAt = null;
        IsInSea = true;
        ReleasedToSeaAt = DateTimeOffset.UtcNow;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new CustomerReleasedToSeaDomainEvent(this));
    }

    /// <summary>
    /// 从公海领用（仅公海客户可领用）
    /// </summary>
    public void ClaimFromSea(UserId ownerId, string? ownerName)
    {
        if (!IsInSea)
            throw new KnownException("仅公海客户可领用", ErrorCodes.CustomerNotInSea);
        OwnerId = ownerId;
        OwnerName = ownerName ?? string.Empty;
        ClaimedAt = DateTimeOffset.UtcNow;
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
