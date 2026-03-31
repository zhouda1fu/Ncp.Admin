using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Services;
using NetCorePal.Context;

namespace Ncp.Admin.Web.Application.Queries;

public record CustomerContactDto(
    CustomerContactId Id,
    string Name,
    string ContactType,
    int Gender,
    DateTimeOffset Birthday,
    string Position,
    string Mobile,
    string Phone,
    string Email,
    bool IsPrimary);

public record CustomerContactRecordDto(
    CustomerContactRecordId Id,
    DateTimeOffset RecordAt,
    string RecordType,
    string Content,
    string RecorderName);

public record CustomerQueryDto(
    CustomerId Id,
    UserId OwnerId,
    DeptId OwnerDeptId,
    string OwnerDeptName,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    bool IsVoided,
    string FullName,
    string ShortName,
    CustomerStatus? Status,
    CompanyNature? Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string ProvinceName,
    string CityName,
    string DistrictName,
    string PhoneProvinceCode,
    string PhoneCityCode,
    string PhoneDistrictCode,
    string PhoneProvinceName,
    string PhoneCityName,
    string PhoneDistrictName,
    string ConsultationContent,
    string ContactQq,
    string ContactWechat,
    string CoverRegion,
    string RegisterAddress,
    int EmployeeCount,
    string BusinessLicense,
    string MainContactName,
    string MainContactPhone,
    string WechatStatus,
    string Remark,
    bool IsKeyAccount,
    bool IsHidden,
    bool CombineFlag,
    bool IsInSea,
    DateTimeOffset? ReleasedToSeaAt,
    UserId CreatorId,
    string CreatorName,
    string OwnerName,
    DateTimeOffset? ClaimedAt,
    DateTimeOffset CreatedAt,
    int ContactCount,
    IReadOnlyList<IndustryId> IndustryIds);

public record CustomerDetailDto(
    CustomerId Id,
    UserId OwnerId,
    DeptId OwnerDeptId,
    string OwnerDeptName,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    bool IsVoided,
    string FullName,
    string ShortName,
    CustomerStatus? Status,
    CompanyNature? Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string ProvinceName,
    string CityName,
    string DistrictName,
    string PhoneProvinceCode,
    string PhoneCityCode,
    string PhoneDistrictCode,
    string PhoneProvinceName,
    string PhoneCityName,
    string PhoneDistrictName,
    string ConsultationContent,
    string ContactQq,
    string ContactWechat,
    string CoverRegion,
    string RegisterAddress,
    int EmployeeCount,
    string BusinessLicense,
    string MainContactName,
    string MainContactPhone,
    string WechatStatus,
    string Remark,
    bool IsKeyAccount,
    bool IsHidden,
    bool CombineFlag,
    bool IsInSea,
    DateTimeOffset? ReleasedToSeaAt,
    UserId CreatorId,
    string CreatorName,
    string OwnerName,
    DateTimeOffset? ClaimedAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<CustomerContactDto> Contacts,
    IReadOnlyList<CustomerContactRecordDto> ContactRecords,
    IReadOnlyList<IndustryId> IndustryIds);

/// <summary>
/// 客户搜索（弹窗）简化 DTO
/// </summary>
public record CustomerSearchDto(CustomerId Id, string FullName, string? ShortName, string? MainContactPhone);

public class CustomerQueryInput : PageRequest
{
    public string? FullName { get; set; }
    public CustomerSourceId? CustomerSourceId { get; set; }
    public bool? IsVoided { get; set; }
    public UserId? OwnerId { get; set; }
    public bool? IsInSea { get; set; }
    public bool? IsKeyAccount { get; set; }
}

public class CustomerSearchInput : PageRequest
{
    public string? Keyword { get; set; }
    public UserId? OwnerId { get; set; }
}

public class CustomerQuery(ApplicationDbContext dbContext, IContextAccessor contextAccessor) : IQuery
{
    public async Task<IReadOnlyList<UserId>> GetSharedToUserIdsAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .SelectMany(c => c.Shares)
            .Select(s => s.SharedToUserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDetailDto?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        var c = await dbContext.Customers
            .AsNoTracking()
            .Include(x => x.Contacts)
            .Include(x => x.ContactRecords)
            .Include(x => x.Industries)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (c == null) return null;
        return new CustomerDetailDto(
            c.Id, c.OwnerId, c.OwnerDeptId, c.OwnerDeptName, c.CustomerSourceId, c.CustomerSourceName, c.IsVoided, c.FullName, c.ShortName, c.Status, c.Nature,
            c.ProvinceCode, c.CityCode, c.DistrictCode, c.ProvinceName, c.CityName, c.DistrictName,
            c.PhoneProvinceCode, c.PhoneCityCode, c.PhoneDistrictCode, c.PhoneProvinceName, c.PhoneCityName, c.PhoneDistrictName,
            c.ConsultationContent, c.ContactQq, c.ContactWechat, c.CoverRegion, c.RegisterAddress, c.EmployeeCount, c.BusinessLicense ?? string.Empty,
            c.MainContactName, c.MainContactPhone, c.WechatStatus, c.Remark, c.IsKeyAccount, c.IsHidden, c.CombineFlag,
            c.IsInSea, c.ReleasedToSeaAt, c.CreatorId, c.CreatorName, c.OwnerName, c.ClaimedAt, c.CreatedAt,
            c.Contacts.Select(x => new CustomerContactDto(x.Id, x.Name, x.ContactType, x.Gender, x.Birthday, x.Position, x.Mobile, x.Phone, x.Email, x.IsPrimary)).ToList(),
            c.ContactRecords.OrderByDescending(r => r.RecordAt).Select(r => new CustomerContactRecordDto(r.Id, r.RecordAt, r.RecordType, r.Content, r.RecorderName)).ToList(),
            c.Industries.Select(x => x.IndustryId).ToList());
    }

    public async Task<PagedData<CustomerQueryDto>> GetPagedAsync(CustomerQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Customers.AsNoTracking();
        // 数据权限过滤：基于当前请求上下文（JWT claims → DataPermissionContext）限制可见客户集合。
        // 说明：客户聚合本身未建模 DeptId，这里采用“按客户负责人(OwnerId)所属部门”作为部门范围过滤口径。
        query = await ApplyDataPermissionAsync(query, cancellationToken);
        if (!string.IsNullOrWhiteSpace(input.FullName))
            query = query.Where(c => c.FullName.Contains(input.FullName));
        if (input.CustomerSourceId != null)
            query = query.Where(c => c.CustomerSourceId == input.CustomerSourceId);
        if (input.IsVoided.HasValue)
            query = query.Where(c => c.IsVoided == input.IsVoided.Value);
        if (input.OwnerId != null)
            query = query.Where(c => c.OwnerId == input.OwnerId);
        if (input.IsInSea.HasValue)
            query = query.Where(c => c.IsInSea == input.IsInSea.Value);
        if (input.IsKeyAccount.HasValue)
            query = query.Where(c => c.IsKeyAccount == input.IsKeyAccount.Value);
        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerQueryDto(
                c.Id, c.OwnerId, c.OwnerDeptId, c.OwnerDeptName, c.CustomerSourceId, c.CustomerSourceName,
                c.IsVoided, c.FullName, c.ShortName, c.Status, c.Nature,
                c.ProvinceCode, c.CityCode, c.DistrictCode, c.ProvinceName, c.CityName, c.DistrictName,
                c.PhoneProvinceCode, c.PhoneCityCode, c.PhoneDistrictCode, c.PhoneProvinceName, c.PhoneCityName, c.PhoneDistrictName,
                c.ConsultationContent, c.ContactQq, c.ContactWechat, c.CoverRegion, c.RegisterAddress, c.EmployeeCount, c.BusinessLicense ?? string.Empty,
                c.MainContactName, c.MainContactPhone, c.WechatStatus, c.Remark, c.IsKeyAccount, c.IsHidden, c.CombineFlag,
                c.IsInSea, c.ReleasedToSeaAt, c.CreatorId, c.CreatorName, c.OwnerName, c.ClaimedAt, c.CreatedAt,
                c.Contacts.Count,
                c.Industries.Select(i => i.IndustryId).ToList()))
            .ToPagedDataAsync(input, cancellationToken);
    }

    public async Task<PagedData<CustomerSearchDto>> SearchAsync(CustomerSearchInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Customers.AsNoTracking()
            .Where(c => !c.IsInSea);
        // 弹窗/搜索场景同样应用数据权限过滤（保证订单“选客户”与客户管理列表一致）。
        query = await ApplyDataPermissionAsync(query, cancellationToken);
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var kw = input.Keyword.Trim();
            query = query.Where(c => c.FullName.Contains(kw) || (c.ShortName != null && c.ShortName.Contains(kw)) || (c.MainContactPhone != null && c.MainContactPhone.Contains(kw)));
        }
        if (input.OwnerId != null)
            query = query.Where(c => c.OwnerId == input.OwnerId);
        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerSearchDto(c.Id, c.FullName, c.ShortName, c.MainContactPhone))
            .ToPagedDataAsync(input, cancellationToken);
    }

    private async Task<IQueryable<Customer>> ApplyDataPermissionAsync(IQueryable<Customer> query, CancellationToken ct)
    {
        var ctx = contextAccessor.GetContext<DataPermissionContext>();
        if (ctx == null || ctx.Scope == DataScope.All)
            return query;

        if (ctx.UserId == null)
            return query.Where(_ => false);

        // 共享可见：共享用户绕过数据范围，直接可见该客户

        if (ctx.Scope == DataScope.Self)
        {
            return query.Where(c => c.Shares.Any(s => s.SharedToUserId == ctx.UserId) || c.OwnerId == ctx.UserId);
        }

        var deptIds = ctx.AuthorizedDeptIds ?? [];
        if (deptIds.Count == 0)
            return query.Where(c => c.Shares.Any(s => s.SharedToUserId == ctx.UserId));

        // 部门范围过滤：直接使用客户冗余的负责人部门字段 OwnerDeptId 做匹配。
        // deptIds 来自登录时写入的 AuthorizedDeptIds（已包含 DeptAndSub / CustomDeptAndSub 的“本部门 + 子部门”展开结果）。
        return query.Where(c =>
            c.Shares.Any(s => s.SharedToUserId == ctx.UserId)
            || (c.OwnerDeptId != new DeptId(0) && deptIds.Contains(c.OwnerDeptId)));
    }
}
