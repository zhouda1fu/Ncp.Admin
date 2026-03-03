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
    string? Description,
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
    string? ProjectEstimate,
    decimal? PurchaseAmount,
    string? ProjectContent);

/// <summary>
/// 项目联系人 DTO
/// </summary>
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
    string? Description,
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
    string? ProjectEstimate,
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
    public Guid? CustomerId { get; set; }
    /// <summary>
    /// 项目类型ID筛选
    /// </summary>
    public Guid? ProjectTypeId { get; set; }
    /// <summary>
    /// 项目状态选项ID筛选
    /// </summary>
    public Guid? ProjectStatusOptionId { get; set; }
    /// <summary>
    /// 项目行业ID筛选
    /// </summary>
    public Guid? ProjectIndustryId { get; set; }
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
                p.Id, p.Name, p.Description, p.CreatorId, p.CreatorName, (int)p.Status, p.CreatedAt,
                p.CustomerId, p.CustomerName, p.ProjectTypeId, p.ProjectTypeName, p.ProjectStatusOptionId, p.ProjectStatusOptionName,
                p.ProjectNumber, p.ProjectIndustryId, p.ProjectIndustryName,
                p.ProvinceRegionId, p.ProvinceName, p.CityRegionId, p.CityName, p.DistrictRegionId, p.DistrictName,
                p.StartDate, p.ProjectEstimate, p.PurchaseAmount, p.ProjectContent))
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
            p.Id, p.Name, p.Description, p.CreatorId, p.CreatorName, (int)p.Status, p.CreatedAt,
            p.CustomerId, p.CustomerName, p.ProjectTypeId, p.ProjectTypeName, p.ProjectStatusOptionId, p.ProjectStatusOptionName,
            p.ProjectNumber, p.ProjectIndustryId, p.ProjectIndustryName,
            p.ProvinceRegionId, p.ProvinceName, p.CityRegionId, p.CityName, p.DistrictRegionId, p.DistrictName,
            p.StartDate, p.ProjectEstimate, p.PurchaseAmount, p.ProjectContent,
            p.Contacts.Select(c => new ProjectContactDto(c.Id, c.CustomerContactId, c.Name, c.Position, c.Mobile, c.OfficePhone, c.QQ, c.Wechat, c.Email, c.IsPrimary, c.Remark)).ToList(),
            p.FollowUpRecords.OrderByDescending(r => r.CreatedAt).Select(r => new ProjectFollowUpRecordDto(r.Id, r.Title, r.VisitDate, r.ReminderIntervalDays, r.Content, r.CreatedAt, r.CreatorId)).ToList());
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
        if (input.CustomerId.HasValue)
            query = query.Where(p => p.CustomerId.Id == input.CustomerId.Value);
        if (input.ProjectTypeId.HasValue)
            query = query.Where(p => p.ProjectTypeId != null && p.ProjectTypeId.Id == input.ProjectTypeId.Value);
        if (input.ProjectStatusOptionId.HasValue)
            query = query.Where(p => p.ProjectStatusOptionId != null && p.ProjectStatusOptionId.Id == input.ProjectStatusOptionId.Value);
        if (input.ProjectIndustryId.HasValue)
            query = query.Where(p => p.ProjectIndustryId.Id == input.ProjectIndustryId.Value);
        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectQueryDto(
                p.Id, p.Name, p.Description, p.CreatorId, p.CreatorName, (int)p.Status, p.CreatedAt,
                p.CustomerId, p.CustomerName, p.ProjectTypeId, p.ProjectTypeName, p.ProjectStatusOptionId, p.ProjectStatusOptionName,
                p.ProjectNumber, p.ProjectIndustryId, p.ProjectIndustryName,
                p.ProvinceRegionId, p.ProvinceName, p.CityRegionId, p.CityName, p.DistrictRegionId, p.DistrictName,
                p.StartDate, p.ProjectEstimate, p.PurchaseAmount, p.ProjectContent))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
