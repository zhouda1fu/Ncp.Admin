using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IUserCalendarMemoRepository : IRepository<UserCalendarMemo, UserCalendarMemoId>
{
    Task<UserCalendarMemo?> GetByUserAndDateAsync(
        UserId userId,
        DateOnly memoDate,
        CancellationToken cancellationToken = default);

    void Remove(UserCalendarMemo memo);
}

public class UserCalendarMemoRepository(ApplicationDbContext context)
    : RepositoryBase<UserCalendarMemo, UserCalendarMemoId, ApplicationDbContext>(context),
        IUserCalendarMemoRepository
{
    public Task<UserCalendarMemo?> GetByUserAndDateAsync(
        UserId userId,
        DateOnly memoDate,
        CancellationToken cancellationToken = default)
    {
        return DbContext.UserCalendarMemos
            .FirstOrDefaultAsync(m => m.UserId == userId && m.MemoDate == memoDate, cancellationToken);
    }

    public void Remove(UserCalendarMemo memo) => DbContext.UserCalendarMemos.Remove(memo);
}
