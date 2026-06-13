using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>用户行事历便签 DTO。</summary>
public record UserCalendarMemoDto(DateOnly MemoDate, string Content);

/// <summary>用户行事历便签查询。</summary>
public sealed class UserCalendarMemoQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<UserCalendarMemoDto?> GetByDateAsync(
        UserId userId,
        DateOnly memoDate,
        CancellationToken cancellationToken = default)
    {
        var memo = await dbContext.UserCalendarMemos.AsNoTracking()
            .Where(m => m.UserId == userId && m.MemoDate == memoDate)
            .Select(m => new UserCalendarMemoDto(m.MemoDate, m.Content))
            .FirstOrDefaultAsync(cancellationToken);

        return memo;
    }
}
