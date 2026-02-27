using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record CustomerContactDto(
    CustomerContactId Id,
    string Name,
    string ContactType,
    int? Gender,
    DateTime? Birthday,
    string Position,
    string Mobile,
    string Phone,
    string Email,
    bool IsPrimary);

public record CustomerQueryDto(
    CustomerId Id,
    UserId? OwnerId,
    DeptId? DeptId,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    int StatusId,
    string FullName,
    string ShortName,
    string Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string CoverRegion,
    string RegisterAddress,
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
    DateTimeOffset CreatedAt,
    int ContactCount,
    IReadOnlyList<IndustryId> IndustryIds);

public record CustomerDetailDto(
    CustomerId Id,
    UserId? OwnerId,
    DeptId? DeptId,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    int StatusId,
    string FullName,
    string ShortName,
    string Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string CoverRegion,
    string RegisterAddress,
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
    DateTimeOffset CreatedAt,
    IReadOnlyList<CustomerContactDto> Contacts,
    IReadOnlyList<IndustryId> IndustryIds);

/// <summary>
/// 客户搜索（弹窗）简化 DTO
/// </summary>
public record CustomerSearchDto(CustomerId Id, string FullName, string? ShortName, string? MainContactPhone);

public class CustomerQueryInput : PageRequest
{
    public string? FullName { get; set; }
    public CustomerSourceId? CustomerSourceId { get; set; }
    public int? StatusId { get; set; }
    public UserId? OwnerId { get; set; }
    public DeptId? DeptId { get; set; }
    public bool? IsInSea { get; set; }
    public bool? IsKeyAccount { get; set; }
}

public class CustomerSearchInput : PageRequest
{
    public string? Keyword { get; set; }
    public UserId? OwnerId { get; set; }
}

public class CustomerQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<CustomerDetailDto?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        var c = await dbContext.Customers
            .AsNoTracking()
            .Include(x => x.Contacts)
            .Include(x => x.Industries)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (c == null) return null;
        return new CustomerDetailDto(
            c.Id, c.OwnerId, c.DeptId, c.CustomerSourceId, c.CustomerSourceName, c.StatusId, c.FullName, c.ShortName, c.Nature,
            c.ProvinceCode, c.CityCode, c.DistrictCode, c.CoverRegion, c.RegisterAddress,
            c.MainContactName, c.MainContactPhone, c.WechatStatus, c.Remark, c.IsKeyAccount, c.IsHidden, c.CombineFlag,
            c.IsInSea, c.ReleasedToSeaAt, c.CreatorId, c.CreatedAt,
            c.Contacts.Select(x => new CustomerContactDto(x.Id, x.Name, x.ContactType, x.Gender, x.Birthday, x.Position, x.Mobile, x.Phone, x.Email, x.IsPrimary)).ToList(),
            c.Industries.Select(x => x.IndustryId).ToList());
    }

    public async Task<PagedData<CustomerQueryDto>> GetPagedAsync(CustomerQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.FullName))
            query = query.Where(c => c.FullName.Contains(input.FullName));
        if (input.CustomerSourceId != null)
            query = query.Where(c => c.CustomerSourceId == input.CustomerSourceId);
        if (input.StatusId.HasValue)
            query = query.Where(c => c.StatusId == input.StatusId.Value);
        if (input.OwnerId != null)
            query = query.Where(c => c.OwnerId == input.OwnerId);
        if (input.DeptId != null)
            query = query.Where(c => c.DeptId == input.DeptId);
        if (input.IsInSea.HasValue)
            query = query.Where(c => c.IsInSea == input.IsInSea.Value);
        if (input.IsKeyAccount.HasValue)
            query = query.Where(c => c.IsKeyAccount == input.IsKeyAccount.Value);
        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerQueryDto(
                c.Id, c.OwnerId, c.DeptId, c.CustomerSourceId, c.CustomerSourceName,
                c.StatusId, c.FullName, c.ShortName, c.Nature,
                c.ProvinceCode, c.CityCode, c.DistrictCode, c.CoverRegion, c.RegisterAddress,
                c.MainContactName, c.MainContactPhone, c.WechatStatus, c.Remark, c.IsKeyAccount, c.IsHidden, c.CombineFlag,
                c.IsInSea, c.ReleasedToSeaAt, c.CreatorId, c.CreatedAt,
                c.Contacts.Count,
                c.Industries.Select(i => i.IndustryId).ToList()))
            .ToPagedDataAsync(input, cancellationToken);
    }

    public async Task<PagedData<CustomerSearchDto>> SearchAsync(CustomerSearchInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Customers.AsNoTracking()
            .Where(c => !c.IsInSea);
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
}
