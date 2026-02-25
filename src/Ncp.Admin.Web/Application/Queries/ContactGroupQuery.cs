using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 联系组查询 DTO
/// </summary>
public record ContactGroupQueryDto(ContactGroupId Id, string Name, UserId CreatorId, int SortOrder, DateTimeOffset CreatedAt);

/// <summary>
/// 联系组分页查询入参
/// </summary>
public class ContactGroupQueryInput : PageRequest
{
    /// <summary>
    /// 名称关键字
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// 联系组查询服务
/// </summary>
public class ContactGroupQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询联系组
    /// </summary>
    public async Task<ContactGroupQueryDto?> GetByIdAsync(ContactGroupId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ContactGroups
            .AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new ContactGroupQueryDto(g.Id, g.Name, g.CreatorId, g.SortOrder, g.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询联系组
    /// </summary>
    public async Task<PagedData<ContactGroupQueryDto>> GetPagedAsync(ContactGroupQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ContactGroups.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Name))
            query = query.Where(g => g.Name.Contains(input.Name));
        return await query
            .OrderBy(g => g.SortOrder)
            .ThenByDescending(g => g.CreatedAt)
            .Select(g => new ContactGroupQueryDto(g.Id, g.Name, g.CreatorId, g.SortOrder, g.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
