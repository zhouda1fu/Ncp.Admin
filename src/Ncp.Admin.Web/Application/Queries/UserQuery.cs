using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 用户信息查询DTO
/// </summary>
public record UserInfoQueryDto(UserId UserId, string Name, string Phone, IEnumerable<string> Roles, string RealName, int Status, string Email, DateTimeOffset CreatedAt, string Gender, int Age, DateTimeOffset BirthDate, DeptId? DeptId, string DeptName);

public record UserLoginInfoQueryDto(UserId UserId, string Name, string Email, string PasswordHash, IEnumerable<UserRole> UserRoles);

public class UserQueryInput : PageRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}

public class UserQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<User> UserSet { get; } = applicationDbContext.Users;

    public async Task<UserId> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
                   .SelectMany(u => u.RefreshTokens)
                   .Where(t => t.Token == refreshToken)
                   .Select(t => t.UserId)
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new KnownException("无效的令牌");
    }

    public async Task<bool> DoesUserExist(string username, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Name == username, cancellationToken: cancellationToken);
    }

    public async Task<bool> DoesUserExist(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken: cancellationToken);
    }

    public async Task<bool> DoesEmailExist(string email, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 根据ID获取用户信息
    /// </summary>
    public async Task<UserInfoQueryDto?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(au => new UserInfoQueryDto(
                au.Id,
                au.Name,
                au.Phone,
                au.Roles.Select(r => r.RoleName),
                au.RealName,
                au.Status,
                au.Email,
                au.CreatedAt,
                au.Gender,
                au.Age,
                au.BirthDate,
                au.Dept != null ? au.Dept.DeptId : null,
                au.Dept != null ? au.Dept.DeptName : string.Empty))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UserId>> GetUserIdsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Roles.Any(r => r.RoleId == roleId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginAsync(string name, CancellationToken cancellationToken)
    {
        return await UserSet
            .Where(u => u.Name == name)
            .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet
            .Where(u => u.Id == userId)
            .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<UserInfoQueryDto>> GetAllUsersAsync(UserQueryInput query, CancellationToken cancellationToken)
    {
        var queryable = UserSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            queryable = queryable.Where(u => u.Name.Contains(query.Keyword!) || u.Email.Contains(query.Keyword!));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(u => u.Status == query.Status);
        }

        return await queryable
            .OrderByDescending(u => u.Id)
            .Select(u => new UserInfoQueryDto(
                u.Id,
                u.Name,
                u.Phone,
                u.Roles.Select(r => r.RoleName),
                u.RealName,
                u.Status,
                u.Email,
                u.CreatedAt,
                u.Gender,
                u.Age,
                u.BirthDate,
                u.Dept != null ? u.Dept.DeptId : null,
                u.Dept != null ? u.Dept.DeptName : string.Empty))
            .ToPagedDataAsync(query, cancellationToken);
    }
}

