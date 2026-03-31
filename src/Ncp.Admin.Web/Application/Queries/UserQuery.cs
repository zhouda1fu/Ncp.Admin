using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 用户信息查询DTO
/// </summary>
public record UserInfoQueryDto(
    UserId UserId,
    string Name,
    string Phone,
    IEnumerable<string> Roles,
    string RealName,
    int Status,
    string Email,
    DateTimeOffset CreatedAt,
    string Gender,
    int Age,
    DateTimeOffset BirthDate,
    DeptId DeptId,
    string DeptName,
    bool IsDeptManager,
    PositionId? PositionId,
    string PositionName,
    string IdCardNumber,
    string Address,
    string Education,
    string GraduateSchool,
    string AvatarUrl,
    bool NotOrderMeal,
    int OrderMealSort,
    string WechatGuid,
    bool IsResigned,
    DateTimeOffset? ResignedTime,
    UserId CreatorId,
    UserId ModifierId,
    UserId DeleterId,
    DateTimeOffset? LastLoginTime,
    string? LastLoginIp);

public record UserLoginInfoQueryDto(UserId UserId, string Name, string Email, string PasswordHash, IEnumerable<UserRole> UserRoles);

public class UserQueryInput : PageRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
    public bool? IsResigned { get; set; }
    /// <summary>按部门筛选用户（与 PositionId 二选一，优先 PositionId）</summary>
    public DeptId? DeptId { get; set; }
    /// <summary>按岗位筛选用户（与 DeptId 二选一，优先 PositionId）</summary>
    public PositionId? PositionId { get; set; }
}

public class UserQuery(ApplicationDbContext applicationDbContext, IMemoryCache memoryCache) : IQuery
{
    private DbSet<User> UserSet { get; } = applicationDbContext.Users;
    private const string UserCacheKeyPrefix = "user:";
    private static readonly TimeSpan UserCacheExpiry = TimeSpan.FromMinutes(10);

    /// <summary>导出允许的最大行数（不含表头）。</summary>
    public const int UserExportMaxRows = 50_000;

    /// <summary>
    /// 获取用户缓存的键（用于更新/删除用户后使缓存失效）
    /// </summary>
    public static string GetUserCacheKey(UserId userId) => $"{UserCacheKeyPrefix}{userId}";

    public async Task<UserId> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
                   .SelectMany(u => u.RefreshTokens)
                   .Where(t => t.Token == refreshToken)
                   .Select(t => t.UserId)
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new KnownException("无效的令牌", ErrorCodes.InvalidToken);
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
    /// 根据ID获取用户信息（带缓存，先查缓存未命中再查库）
    /// </summary>
    public async Task<UserInfoQueryDto> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{UserCacheKeyPrefix}{userId}";

        var result = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = UserCacheExpiry;
            var user = await UserSet.AsNoTracking()
                .Where(u => u.Id == userId)
                .Where(u => !u.IsResigned)
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
                    au.Dept != null ? au.Dept.DeptId : new DeptId(0),
                    au.Dept != null ? au.Dept.DeptName : string.Empty,
                    au.Dept != null && au.Dept.IsDeptManager,
                    au.Position != null ? au.Position.PositionId : (PositionId?)null,
                    au.Position != null ? au.Position.PositionName : string.Empty,
                    au.IdCardNumber,
                    au.Address,
                    au.Education,
                    au.GraduateSchool,
                    au.AvatarUrl,
                    au.NotOrderMeal,
                    au.OrderMealSort,
                    au.WechatGuid,
                    au.IsResigned,
                    au.ResignedTime,
                    au.CreatorId,
                    au.ModifierId,
                    au.DeleterId,
                    au.LastLoginTime,
                    au.LastLoginIp))
                .FirstOrDefaultAsync(cancellationToken);
            return user ?? throw new KnownException("用户不存在", ErrorCodes.UserNotFound);
        });
        return result!;
    }

    public async Task<List<UserId>> GetUserIdsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Roles.Any(r => r.RoleId == roleId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据角色ID获取该角色下所有用户（UserId + 显示名），用于会签按人创建任务
    /// </summary>
    public async Task<List<(UserId Id, string DisplayName)>> GetUserAssigneesByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Roles.Any(r => r.RoleId == roleId))
            .Select(u => new ValueTuple<UserId, string>(u.Id, u.RealName != null && u.RealName.Length > 0 ? u.RealName : u.Name))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据用户ID获取其所属角色ID列表（用于待办按角色查询）
    /// </summary>
    public async Task<List<RoleId>> GetRoleIdsByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .Select(r => r.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据部门ID获取所有用户ID列表
    /// </summary>
    /// <param name="deptId">部门ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>属于指定部门的所有用户ID列表</returns>
    public async Task<List<UserId>> GetUserIdsByDeptIdAsync(DeptId deptId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Dept != null && u.Dept.DeptId == deptId)
            .Where(u=>!u.IsResigned)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据多个部门ID获取所有用户ID列表（去重，排除离职用户）
    /// </summary>
    public async Task<List<UserId>> GetUserIdsByDeptIdsAsync(IEnumerable<DeptId> deptIds, CancellationToken cancellationToken = default)
    {
        var ids = (deptIds ?? Enumerable.Empty<DeptId>()).Distinct().ToList();
        if (ids.Count == 0) return [];

        return await UserSet.AsNoTracking()
            .Where(u => !u.IsResigned)
            .Where(u => u.Dept != null && ids.Contains(u.Dept.DeptId))
            .Select(u => u.Id)
            .Distinct()
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
        var queryable = ApplyUserListFilters(UserSet.AsNoTracking(), query);
        return await queryable
            .OrderByDescending(u => u.Id)
            .Select(ToUserInfoQueryDto())
            .ToPagedDataAsync(query, cancellationToken);
    }

    /// <summary>
    /// 按与列表相同的筛选条件导出（不分页）；超过 <see cref="UserExportMaxRows"/> 时抛出业务异常。
    /// </summary>
    public async Task<IReadOnlyList<UserInfoQueryDto>> GetUsersForExportAsync(UserQueryInput filter, CancellationToken cancellationToken)
    {
        var queryable = ApplyUserListFilters(UserSet.AsNoTracking(), filter);
        var take = UserExportMaxRows + 1;
        var list = await queryable
            .OrderByDescending(u => u.Id)
            .Select(ToUserInfoQueryDto())
            .Take(take)
            .ToListAsync(cancellationToken);
        if (list.Count > UserExportMaxRows)
        {
            throw new KnownException(
                $"导出数据超过上限 {UserExportMaxRows} 条，请缩小筛选条件后重试",
                ErrorCodes.UserExportTooManyRows);
        }

        return list;
    }

    private static IQueryable<User> ApplyUserListFilters(IQueryable<User> queryable, UserQueryInput query)
    {
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            queryable = queryable.Where(u => u.Name.Contains(query.Keyword!) || u.Email.Contains(query.Keyword!));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(u => u.Status == query.Status);
        }

        if (query.IsResigned.HasValue)
        {
            queryable = queryable.Where(u => u.IsResigned == query.IsResigned.Value);
        }

        if (query.PositionId != null)
        {
            queryable = queryable.Where(u => u.Position != null && u.Position.PositionId == query.PositionId);
        }
        else if (query.DeptId != null)
        {
            queryable = queryable.Where(u => u.Dept != null && u.Dept.DeptId == query.DeptId);
        }

        return queryable;
    }

    private static Expression<Func<User, UserInfoQueryDto>> ToUserInfoQueryDto()
    {
        return u => new UserInfoQueryDto(
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
            u.Dept != null ? u.Dept.DeptId : new DeptId(0),
            u.Dept != null ? u.Dept.DeptName : string.Empty,
            u.Dept != null && u.Dept.IsDeptManager,
            u.Position != null ? u.Position.PositionId : (PositionId?)null,
            u.Position != null ? u.Position.PositionName : string.Empty,
            u.IdCardNumber,
            u.Address,
            u.Education,
            u.GraduateSchool,
            u.AvatarUrl,
            u.NotOrderMeal,
            u.OrderMealSort,
            u.WechatGuid,
            u.IsResigned,
            u.ResignedTime,
            u.CreatorId,
            u.ModifierId,
            u.DeleterId,
            u.LastLoginTime,
            u.LastLoginIp);
    }
}

