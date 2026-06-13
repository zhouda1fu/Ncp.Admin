using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface IUserHomeDashboardPreferenceRepository : IRepository<UserHomeDashboardPreference, UserId>
{
    Task<UserHomeDashboardPreference?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);
}

public class UserHomeDashboardPreferenceRepository(ApplicationDbContext context)
    : RepositoryBase<UserHomeDashboardPreference, UserId, ApplicationDbContext>(context),
        IUserHomeDashboardPreferenceRepository
{
    public Task<UserHomeDashboardPreference?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return DbContext.UserHomeDashboardPreferences
            .FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
    }
}
