using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 联系人查询 DTO
/// </summary>
public record ContactQueryDto(
    ContactId Id,
    string Name,
    string? Phone,
    string? Email,
    string? Company,
    ContactGroupId? GroupId,
    UserId CreatorId,
    DateTimeOffset CreatedAt);

/// <summary>
/// 联系人分页查询入参
/// </summary>
public class ContactQueryInput : PageRequest
{
    /// <summary>
    /// 姓名/电话/邮箱关键字
    /// </summary>
    public string? Keyword { get; set; }
    /// <summary>
    /// 分组ID筛选
    /// </summary>
    public Guid? GroupId { get; set; }
}

/// <summary>
/// 联系人查询服务
/// </summary>
public class ContactQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询联系人
    /// </summary>
    public async Task<ContactQueryDto?> GetByIdAsync(ContactId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Contacts
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ContactQueryDto(c.Id, c.Name, c.Phone, c.Email, c.Company, c.GroupId, c.CreatorId, c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询联系人
    /// </summary>
    public async Task<PagedData<ContactQueryDto>> GetPagedAsync(ContactQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Contacts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var kw = input.Keyword.Trim();
            query = query.Where(c =>
                c.Name.Contains(kw) ||
                (c.Phone != null && c.Phone.Contains(kw)) ||
                (c.Email != null && c.Email.Contains(kw)));
        }
        if (input.GroupId.HasValue)
        {
            var gid = new ContactGroupId(input.GroupId.Value);
            query = query.Where(c => c.GroupId == gid);
        }
        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContactQueryDto(c.Id, c.Name, c.Phone, c.Email, c.Company, c.GroupId, c.CreatorId, c.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
