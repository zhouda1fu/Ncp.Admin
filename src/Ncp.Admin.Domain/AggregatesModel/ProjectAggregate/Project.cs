using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

/// <summary>
/// 项目ID（强类型ID）
/// </summary>
public partial record ProjectId : IGuidStronglyTypedId;

/// <summary>
/// 项目状态（用于归档/激活）
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// 进行中
    /// </summary>
    Active = 0,
    /// <summary>
    /// 已归档
    /// </summary>
    Archived = 1,
}

/// <summary>
/// 项目聚合根，用于任务看板与协作，与客户绑定
/// </summary>
public class Project : Entity<ProjectId>, IAggregateRoot
{
    protected Project() { }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 项目描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;
    /// <summary>
    /// 创建人用户姓名（冗余，便于列表/展示）
    /// </summary>
    public string CreatorName { get; private set; } = string.Empty;
    /// <summary>
    /// 项目状态（归档/激活）
    /// </summary>
    public ProjectStatus Status { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 客户ID（必填，项目归属客户）
    /// </summary>
    public CustomerId CustomerId { get; private set; } = default!;
    /// <summary>
    /// 客户姓名（冗余，便于列表/展示）
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;
    /// <summary>
    /// 项目类型ID（必填）
    /// </summary>
    public ProjectTypeId ProjectTypeId { get; private set; } = default!;
    /// <summary>
    /// 项目类型名称（冗余，便于列表/展示）
    /// </summary>
    public string ProjectTypeName { get; private set; } = string.Empty;
    /// <summary>
    /// 项目状态选项ID（必填，业务状态，如新项目/进行中）
    /// </summary>
    public ProjectStatusOptionId ProjectStatusOptionId { get; private set; } = default!;
    /// <summary>
    /// 项目状态选项名称（冗余，便于列表/展示）
    /// </summary>
    public string ProjectStatusOptionName { get; private set; } = string.Empty;
    /// <summary>
    /// 项目编号
    /// </summary>
    public string ProjectNumber { get; private set; } = string.Empty;
    /// <summary>
    /// 项目行业ID（必填）
    /// </summary>
    public ProjectIndustryId ProjectIndustryId { get; private set; } = default!;
    /// <summary>
    /// 项目行业名称（冗余，便于列表/展示）
    /// </summary>
    public string ProjectIndustryName { get; private set; } = string.Empty;
    /// <summary>
    /// 项目所在地-省 RegionId（必填）
    /// </summary>
    public RegionId ProvinceRegionId { get; private set; } = default!;
    /// <summary>
    /// 项目所在地-省名称（冗余）
    /// </summary>
    public string ProvinceName { get; private set; } = string.Empty;
    /// <summary>
    /// 项目所在地-市 RegionId（必填）
    /// </summary>
    public RegionId CityRegionId { get; private set; } = default!;
    /// <summary>
    /// 项目所在地-市名称（冗余）
    /// </summary>
    public string CityName { get; private set; } = string.Empty;
    /// <summary>
    /// 项目所在地-区 RegionId（必填）
    /// </summary>
    public RegionId DistrictRegionId { get; private set; } = default!;
    /// <summary>
    /// 项目所在地-区名称（冗余）
    /// </summary>
    public string DistrictName { get; private set; } = string.Empty;
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateOnly? StartDate { get; private set; }
    /// <summary>
    /// 项目预估（文本或金额说明）
    /// </summary>
    public string ProjectEstimate { get; private set; } = string.Empty;
    /// <summary>
    /// 采购金额
    /// </summary>
    public decimal PurchaseAmount { get; private set; }
    /// <summary>
    /// 项目内容（长文本）
    /// </summary>
    public string ProjectContent { get; private set; } = string.Empty;

    /// <summary>
    /// 项目联系人列表
    /// </summary>
    public virtual ICollection<ProjectContact> Contacts { get; } = [];

    /// <summary>
    /// 项目跟进记录列表
    /// </summary>
    public virtual ICollection<ProjectFollowUpRecord> FollowUpRecords { get; } = [];

    /// <summary>
    /// 创建项目
    /// </summary>
    public Project(
        CustomerId customerId,
        string customerName,
        ProjectTypeId projectTypeId,
        string projectTypeName,
        ProjectStatusOptionId projectStatusOptionId,
        string projectStatusOptionName,
        ProjectIndustryId projectIndustryId,
        string projectIndustryName,
        RegionId provinceRegionId,
        string provinceName,
        RegionId cityRegionId,
        string cityName,
        RegionId districtRegionId,
        string districtName,
        string name,
        UserId creatorId,
        string creatorName,
        string description,
        string projectNumber = "",
        DateOnly? startDate = null,
        string projectEstimate = "",
        decimal purchaseAmount = 0,
        string projectContent = "")
    {
        CustomerId = customerId;
        CustomerName = customerName ?? string.Empty;
        ProjectTypeId = projectTypeId;
        ProjectTypeName = projectTypeName ?? string.Empty;
        ProjectStatusOptionId = projectStatusOptionId;
        ProjectStatusOptionName = projectStatusOptionName ?? string.Empty;
        ProjectIndustryId = projectIndustryId;
        ProjectIndustryName = projectIndustryName ?? string.Empty;
        ProvinceRegionId = provinceRegionId;
        ProvinceName = provinceName ?? string.Empty;
        CityRegionId = cityRegionId;
        CityName = cityName ?? string.Empty;
        DistrictRegionId = districtRegionId;
        DistrictName = districtName ?? string.Empty;
        Name = name;
        CreatorId = creatorId;
        CreatorName = creatorName ?? string.Empty;
        Description = description ?? string.Empty;
        ProjectNumber = projectNumber ?? string.Empty;
        StartDate = startDate;
        ProjectEstimate = projectEstimate ?? string.Empty;
        PurchaseAmount = purchaseAmount;
        ProjectContent = projectContent ?? string.Empty;
        Status = ProjectStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新项目信息
    /// </summary>
    public void Update(
        string name,
        string description,
        ProjectTypeId projectTypeId,
        string projectTypeName,
        ProjectStatusOptionId projectStatusOptionId,
        string projectStatusOptionName,
        string projectNumber,
        ProjectIndustryId projectIndustryId,
        string projectIndustryName,
        RegionId provinceRegionId,
        string provinceName,
        RegionId cityRegionId,
        string cityName,
        RegionId districtRegionId,
        string districtName,
        DateOnly? startDate,
        string projectEstimate,
        decimal purchaseAmount,
        string projectContent)
    {
        Name = name;
        Description = description ?? string.Empty;
        ProjectTypeId = projectTypeId;
        ProjectTypeName = projectTypeName ?? string.Empty;
        ProjectStatusOptionId = projectStatusOptionId;
        ProjectStatusOptionName = projectStatusOptionName ?? string.Empty;
        ProjectNumber = projectNumber ?? string.Empty;
        ProjectIndustryId = projectIndustryId;
        ProjectIndustryName = projectIndustryName ?? string.Empty;
        ProvinceRegionId = provinceRegionId;
        ProvinceName = provinceName ?? string.Empty;
        CityRegionId = cityRegionId;
        CityName = cityName ?? string.Empty;
        DistrictRegionId = districtRegionId;
        DistrictName = districtName ?? string.Empty;
        StartDate = startDate;
        ProjectEstimate = projectEstimate ?? string.Empty;
        PurchaseAmount = purchaseAmount;
        ProjectContent = projectContent ?? string.Empty;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 归档项目
    /// </summary>
    public void Archive()
    {
        if (Status == ProjectStatus.Archived)
            return;
        Status = ProjectStatus.Archived;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 激活项目
    /// </summary>
    public void Activate()
    {
        if (Status == ProjectStatus.Active)
            return;
        Status = ProjectStatus.Active;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 添加项目联系人
    /// </summary>
    public ProjectContactId AddContact(
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
        var contact = ProjectContact.Create(Id, customerContactId, name, position, mobile, officePhone, qq, wechat, email, isPrimary, remark);
        Contacts.Add(contact);
        if (isPrimary)
        {
            foreach (var c in Contacts.Where(c => c != contact))
                c.Update(c.CustomerContactId, c.Name, c.Position, c.Mobile, c.OfficePhone, c.QQ, c.Wechat, c.Email, false, c.Remark);
        }
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        return contact.Id;
    }

    /// <summary>
    /// 更新指定项目联系人
    /// </summary>
    public void UpdateContact(
        ProjectContactId contactId,
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
        var contact = Contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new KnownException("未找到项目联系人", ErrorCodes.ProjectContactNotFound);
        contact.Update(customerContactId, name, position, mobile, officePhone, qq, wechat, email, isPrimary, remark);
        if (isPrimary)
        {
            foreach (var c in Contacts.Where(c => c != contact))
                c.Update(c.CustomerContactId, c.Name, c.Position, c.Mobile, c.OfficePhone, c.QQ, c.Wechat, c.Email, false, c.Remark);
        }
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 移除指定项目联系人
    /// </summary>
    public void RemoveContact(ProjectContactId contactId)
    {
        var contact = Contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new KnownException("未找到项目联系人", ErrorCodes.ProjectContactNotFound);
        Contacts.Remove(contact);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 添加项目跟进记录
    /// </summary>
    public ProjectFollowUpRecordId AddFollowUpRecord(
        string title,
        DateOnly? visitDate,
        int reminderIntervalDays,
        string content,
        UserId? creatorId)
    {
        var record = ProjectFollowUpRecord.Create(Id, title, visitDate, reminderIntervalDays, content, creatorId);
        FollowUpRecords.Add(record);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        return record.Id;
    }

    /// <summary>
    /// 更新指定项目跟进记录
    /// </summary>
    public void UpdateFollowUpRecord(
        ProjectFollowUpRecordId recordId,
        string title,
        DateOnly? visitDate,
        int reminderIntervalDays,
        string content)
    {
        var record = FollowUpRecords.FirstOrDefault(r => r.Id == recordId)
            ?? throw new KnownException("未找到项目跟进记录", ErrorCodes.ProjectFollowUpRecordNotFound);
        record.Update(title, visitDate, reminderIntervalDays, content);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 移除指定项目跟进记录
    /// </summary>
    public void RemoveFollowUpRecord(ProjectFollowUpRecordId recordId)
    {
        var record = FollowUpRecords.FirstOrDefault(r => r.Id == recordId)
            ?? throw new KnownException("未找到项目跟进记录", ErrorCodes.ProjectFollowUpRecordNotFound);
        FollowUpRecords.Remove(record);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
