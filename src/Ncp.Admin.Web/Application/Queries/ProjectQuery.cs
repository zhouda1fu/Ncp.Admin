using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 项目查询 DTO
/// </summary>
public record ProjectQueryDto(
    ProjectId Id,
    string Name,
    UserId CreatorId,
    string CreatorName,
    int Status,
    DateTimeOffset CreatedAt,
    CustomerId CustomerId,
    string CustomerName,
    ProjectTypeId ProjectTypeId,
    string ProjectTypeName,
    ProjectStatusOptionId ProjectStatusOptionId,
    string ProjectStatusOptionName,
    string? ProjectNumber,
    ProjectIndustryId ProjectIndustryId,
    string ProjectIndustryName,
    RegionId ProvinceRegionId,
    string ProvinceName,
    RegionId CityRegionId,
    string CityName,
    RegionId DistrictRegionId,
    string DistrictName,
    DateOnly? StartDate,
    decimal Budget,
    decimal? PurchaseAmount,
    string? ProjectContent);

/// <summary>
/// 项目联系人 DTO
/// </summary>
/// <param name="Id">联系人 ID</param>
/// <param name="CustomerContactId">关联的客户联系人，未关联为 null</param>
/// <param name="Name">姓名</param>
/// <param name="Position">职位</param>
/// <param name="Mobile">手机</param>
/// <param name="OfficePhone">办公电话</param>
/// <param name="QQ">QQ</param>
/// <param name="Wechat">微信</param>
/// <param name="Email">邮箱</param>
/// <param name="IsPrimary">是否主要联系人</param>
/// <param name="Remark">备注</param>
public record ProjectContactDto(
    ProjectContactId Id,
    CustomerContactId? CustomerContactId,
    string Name,
    string Position,
    string Mobile,
    string OfficePhone,
    string QQ,
    string Wechat,
    string Email,
    bool IsPrimary,
    string Remark);

/// <summary>
/// 项目跟进记录 DTO
/// </summary>
/// <param name="Id">记录 ID</param>
/// <param name="Title">标题</param>
/// <param name="VisitDate">拜访日期</param>
/// <param name="ReminderIntervalDays">提醒间隔天数</param>
/// <param name="Content">内容</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="CreatorId">创建人，未知为 null</param>
public record ProjectFollowUpRecordDto(
    ProjectFollowUpRecordId Id,
    string Title,
    DateOnly? VisitDate,
    int ReminderIntervalDays,
    string Content,
    DateTimeOffset CreatedAt,
    UserId? CreatorId);

/// <summary>
/// 项目详情 DTO（含联系人与跟进记录）
/// </summary>
public record ProjectDetailDto(
    ProjectId Id,
    string Name,
    UserId CreatorId,
    string CreatorName,
    int Status,
    DateTimeOffset CreatedAt,
    CustomerId CustomerId,
    string CustomerName,
    ProjectTypeId ProjectTypeId,
    string ProjectTypeName,
    ProjectStatusOptionId ProjectStatusOptionId,
    string ProjectStatusOptionName,
    string? ProjectNumber,
    ProjectIndustryId ProjectIndustryId,
    string ProjectIndustryName,
    RegionId ProvinceRegionId,
    string ProvinceName,
    RegionId CityRegionId,
    string CityName,
    RegionId DistrictRegionId,
    string DistrictName,
    DateOnly? StartDate,
    decimal Budget,
    decimal? PurchaseAmount,
    string? ProjectContent,
    IReadOnlyList<ProjectContactDto> Contacts,
    IReadOnlyList<ProjectFollowUpRecordDto> FollowUpRecords);

/// <summary>
/// 项目分页查询入参
/// </summary>
public class ProjectQueryInput : PageRequest
{
    /// <summary>
    /// 名称关键字
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 状态筛选（0 进行中 1 已归档）
    /// </summary>
    public int? Status { get; set; }
    /// <summary>
    /// 客户ID筛选
    /// </summary>
    public CustomerId? CustomerId { get; set; }
    /// <summary>
    /// 项目类型ID筛选
    /// </summary>
    public ProjectTypeId? ProjectTypeId { get; set; }
    /// <summary>
    /// 项目状态选项ID筛选
    /// </summary>
    public ProjectStatusOptionId? ProjectStatusOptionId { get; set; }
    /// <summary>
    /// 项目行业ID筛选
    /// </summary>
    public ProjectIndustryId? ProjectIndustryId { get; set; }
}

/// <summary>
/// 项目查询服务
/// </summary>
public class ProjectQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询项目（列表/简单用）
    /// </summary>
    public async Task<ProjectQueryDto?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProjectQueryDto(
                p.Id, p.Name, p.CreatorId, p.CreatorName, (int)p.Status, p.CreatedAt,
                p.CustomerId, p.CustomerName, p.ProjectTypeId, p.ProjectTypeName, p.ProjectStatusOptionId, p.ProjectStatusOptionName,
                p.ProjectNumber, p.ProjectIndustryId, p.ProjectIndustryName,
                p.ProvinceRegionId, p.ProvinceName, p.CityRegionId, p.CityName, p.DistrictRegionId, p.DistrictName,
                p.StartDate, p.Budget, p.PurchaseAmount, p.ProjectContent))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 按 ID 查询项目详情（含联系人、跟进记录，编辑页用）
    /// </summary>
    public async Task<ProjectDetailDto?> GetDetailByIdAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        var p = await dbContext.Projects
            .AsNoTracking()
            .Include(x => x.Contacts)
            .Include(x => x.FollowUpRecords)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (p == null) return null;
        return new ProjectDetailDto(
            p.Id, p.Name, p.CreatorId, p.CreatorName, (int)p.Status, p.CreatedAt,
            p.CustomerId, p.CustomerName, p.ProjectTypeId, p.ProjectTypeName, p.ProjectStatusOptionId, p.ProjectStatusOptionName,
            p.ProjectNumber, p.ProjectIndustryId, p.ProjectIndustryName,
            p.ProvinceRegionId, p.ProvinceName, p.CityRegionId, p.CityName, p.DistrictRegionId, p.DistrictName,
            p.StartDate, p.Budget, p.PurchaseAmount, p.ProjectContent,
            p.Contacts.Select(c => new ProjectContactDto(
                c.Id,
                c.CustomerContactId == ProjectContact.NoLinkedCustomerContactId ? null : c.CustomerContactId,
                c.Name,
                c.Position,
                c.Mobile,
                c.OfficePhone,
                c.QQ,
                c.Wechat,
                c.Email,
                c.IsPrimary,
                c.Remark)).ToList(),
            p.FollowUpRecords.OrderByDescending(r => r.CreatedAt).Select(r => new ProjectFollowUpRecordDto(
                r.Id,
                r.Title,
                r.VisitDate,
                r.ReminderIntervalDays,
                r.Content,
                r.CreatedAt,
                r.CreatorId == new UserId(0) ? null : r.CreatorId)).ToList());
    }

    /// <summary>
    /// 分页查询项目
    /// </summary>
    public async Task<PagedData<ProjectQueryDto>> GetPagedAsync(ProjectQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Projects.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Name))
            query = query.Where(p => p.Name.Contains(input.Name));
        if (input.Status.HasValue)
            query = query.Where(p => (int)p.Status == input.Status.Value);
        var filterCustomerId = input.CustomerId;
        if (filterCustomerId != null)
            query = query.Where(p => p.CustomerId == filterCustomerId);
        var filterProjectTypeId = input.ProjectTypeId;
        if (filterProjectTypeId != null)
            query = query.Where(p => p.ProjectTypeId == filterProjectTypeId);
        var filterProjectStatusOptionId = input.ProjectStatusOptionId;
        if (filterProjectStatusOptionId != null)
            query = query.Where(p => p.ProjectStatusOptionId == filterProjectStatusOptionId);
        var filterProjectIndustryId = input.ProjectIndustryId;
        if (filterProjectIndustryId != null)
            query = query.Where(p => p.ProjectIndustryId == filterProjectIndustryId);
        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectQueryDto(
                p.Id, p.Name, p.CreatorId, p.CreatorName, (int)p.Status, p.CreatedAt,
                p.CustomerId, p.CustomerName, p.ProjectTypeId, p.ProjectTypeName, p.ProjectStatusOptionId, p.ProjectStatusOptionName,
                p.ProjectNumber, p.ProjectIndustryId, p.ProjectIndustryName,
                p.ProvinceRegionId, p.ProvinceName, p.CityRegionId, p.CityName, p.DistrictRegionId, p.DistrictName,
                p.StartDate, p.Budget, p.PurchaseAmount, p.ProjectContent))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
