using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Web.Application.Services.Workflow;

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
    PositionId? PositionId,
    string PositionName,
    string IdCardNumber,
    string Address,
    string Education,
    string GraduateSchool,
    string AvatarUrl,
    bool NotOrderMeal,
    int OrderMealSort,
    bool AttendanceRequired,
    string WechatGuid,
    bool IsResigned,
    DateTimeOffset? ResignedTime,
    UserId CreatorId,
    UserId ModifierId,
    UserId DeleterId,
    DateTimeOffset? LastLoginTime,
    string? LastLoginIp,
    bool SetAsDeptResponsibleUser,
    bool SetAsDefaultDeptResponsibleUser)
{
    /// <summary>
    /// 前端使用字符串承载雪花 ID，避免 JavaScript Number 精度丢失。
    /// </summary>
    public string UserIdText => UserId.Id.ToString();
}

public record UserLoginInfoQueryDto(UserId UserId, string Name, string Email, string PasswordHash, IEnumerable<UserRole> UserRoles);
public record UserDataPermissionSnapshot(UserId UserId, DeptId DeptId, IReadOnlyList<RoleId> RoleIds);

public class UserQueryInput : PageRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
    public bool? IsResigned { get; set; }
    /// <summary>按部门筛选用户（与 PositionId 二选一，优先 PositionId）</summary>
    public DeptId? DeptId { get; set; }
    /// <summary>按岗位筛选用户（与 DeptId 二选一，优先 PositionId）</summary>
    public PositionId? PositionId { get; set; }
    /// <summary>
    /// 为 true 时仅返回「营销中心」及其下级部门用户，并忽略请求中的 <see cref="DeptId"/> / <see cref="PositionId"/>（客户协作转交/分享选人等）。
    /// </summary>
    public bool OnlyMarketingCenterDeptSubtree { get; set; }

    /// <summary>
    /// 为 true 时仅返回「技术部」及其下级部门用户，并忽略请求中的 <see cref="DeptId"/> / <see cref="PositionId"/>（技术分配选人等）。
    /// </summary>
    public bool OnlyTechnologyDeptSubtree { get; set; }
    /// <summary>
    /// 为 true 时仅返回值日可选部门且参与考勤的用户（「产品研发中心」「网络推广组」及其下级），并忽略请求中的 <see cref="DeptId"/> / <see cref="PositionId"/>（值日安排选人等）。
    /// </summary>
    public bool OnlyProductResearchCenterDeptSubtree { get; set; }

    /// <summary>表头筛选：部门名称多选（精确匹配 <see cref="UserDept.DeptName"/>）。</summary>
    public IReadOnlyList<string>? FilterDeptNames { get; set; }

    /// <summary>表头筛选：角色名称多选（用户拥有任一选中角色即命中）。</summary>
    public IReadOnlyList<string>? FilterRoleNames { get; set; }

}

public class UserQuery(ApplicationDbContext applicationDbContext, IMemoryCache memoryCache, DeptQuery deptQuery) : IQuery
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
    /// 根据ID获取用户信息（带缓存，先查缓存未命中再查库）。
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="includeResigned">为 true 时包含已离职用户（如管理端用户编辑）；默认 false，与历史行为一致（离职视为不可用）。</param>
    public async Task<UserInfoQueryDto> GetUserByIdAsync(
        UserId userId,
        CancellationToken cancellationToken,
        bool includeResigned = false)
    {
        var cacheKey = $"{UserCacheKeyPrefix}{userId}";

        var result = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = UserCacheExpiry;
            var user = await UserSet.AsNoTracking()
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
                    0,
                    au.BirthDate,
                    au.Dept != null ? au.Dept.DeptId : DeptId.Unassigned,
                    au.Dept != null ? au.Dept.DeptName : string.Empty,
                    au.Position != null ? au.Position.PositionId : (PositionId?)null,
                    au.Position != null ? au.Position.PositionName : string.Empty,
                    au.IdCardNumber,
                    au.Address,
                    au.Education,
                    au.GraduateSchool,
                    au.AvatarUrl,
                    au.NotOrderMeal,
                    au.OrderMealSort,
                    au.AttendanceRequired,
                    au.WechatGuid,
                    au.IsResigned,
                    au.ResignedTime,
                    au.CreatorId,
                    au.ModifierId,
                    au.DeleterId,
                    au.LastLoginTime,
                    au.LastLoginIp,
                    au.Dept != null && applicationDbContext.DeptResponsibleUsers
                        .Any(r => r.DeptId == au.Dept.DeptId && r.UserId == au.Id),
                    au.Dept != null && applicationDbContext.DeptResponsibleUsers
                        .Any(r => r.DeptId == au.Dept.DeptId && r.UserId == au.Id && r.IsDefault)))
                .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
                throw new KnownException("用户不存在", ErrorCodes.UserNotFound);
            return user;
        });
        if (!includeResigned && result!.IsResigned)
            throw new KnownException("用户不存在", ErrorCodes.UserNotFound);
        return result! with { Age = User.CalculateAge(result.BirthDate) };
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
            .Where(u => !u.IsResigned)
            .Select(u => new ValueTuple<UserId, string>(u.Id, u.RealName != null && u.RealName.Length > 0 ? u.RealName : u.Name))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据角色权限码获取可用用户，用于工作流管理员兜底。
    /// </summary>
    public async Task<List<(UserId Id, string DisplayName)>> GetUserAssigneesByPermissionCodeAsync(
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return [];
        }

        var roleIds = await applicationDbContext.RolePermissions.AsNoTracking()
            .Where(p => p.PermissionCode == permissionCode)
            .Select(p => p.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (roleIds.Count == 0)
        {
            return [];
        }

        return await UserSet.AsNoTracking()
            .Where(u => !u.IsResigned)
            .Where(u => u.Roles.Any(r => roleIds.Contains(r.RoleId)))
            .OrderBy(u => u.Id)
            .Select(u => new ValueTuple<UserId, string>(u.Id, u.RealName != null && u.RealName.Length > 0 ? u.RealName : u.Name))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据多个权限码获取可用用户 ID。
    /// </summary>
    public async Task<IReadOnlyList<long>> GetUserIdsByPermissionCodesAsync(
        IReadOnlyList<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        var codes = (permissionCodes ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (codes.Count == 0)
        {
            return [];
        }

        var roleIds = await applicationDbContext.RolePermissions.AsNoTracking()
            .Where(p => codes.Contains(p.PermissionCode))
            .Select(p => p.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (roleIds.Count == 0)
        {
            return [];
        }

        return await UserSet.AsNoTracking()
            .Where(u => !u.IsDeleted && !u.IsResigned && u.IsActive && u.Status == 1)
            .Where(u => u.Roles.Any(r => roleIds.Contains(r.RoleId)))
            .Select(u => u.Id.Id)
            .Distinct()
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
    /// 批量获取用户的数据权限判定快照（部门 + 角色），用于减少审批可见性判定时的N+1查询。
    /// </summary>
    public async Task<List<UserDataPermissionSnapshot>> GetDataPermissionSnapshotsByUserIdsAsync(
        IEnumerable<UserId> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = (userIds ?? Enumerable.Empty<UserId>()).Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var rows = await UserSet.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Where(u => !u.IsResigned)
            .Select(u => new
            {
                u.Id,
                DeptId = u.Dept != null ? u.Dept.DeptId : DeptId.Unassigned,
                RoleIds = u.Roles.Select(r => r.RoleId).Distinct().ToList(),
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new UserDataPermissionSnapshot(r.Id, r.DeptId, r.RoleIds))
            .ToList();
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
            .Where(u => u.Status == 1 && u.IsActive)
            .Where(u => !u.IsResigned)
            .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet
            .Where(u => u.Id == userId)
            .Where(u => u.Status == 1 && u.IsActive)
            .Where(u => !u.IsResigned)
            .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<UserInfoQueryDto>> GetAllUsersAsync(UserQueryInput query, CancellationToken cancellationToken)
    {
        var deptSubtreeFilter = await ResolveDeptSubtreeFilterAsync(query, cancellationToken);

        var queryable = ApplyUserListFilters(UserSet.AsNoTracking(), query, deptSubtreeFilter, false, false);
        var page = await queryable
            .OrderByDescending(u => u.Id)
            .Select(ToUserInfoQueryDto())
            .ToPagedDataAsync(query, cancellationToken);

        var items = page.Items.Select(i => i with { Age = User.CalculateAge(i.BirthDate) }).ToList();
        return new PagedData<UserInfoQueryDto>(items, page.Total, query.PageIndex, query.PageSize);
    }

    /// <summary>
    /// 按与列表相同的筛选条件导出（不分页）；超过 <see cref="UserExportMaxRows"/> 时抛出业务异常。
    /// </summary>
    public async Task<IReadOnlyList<UserInfoQueryDto>> GetUsersForExportAsync(UserQueryInput filter, CancellationToken cancellationToken)
    {
        var deptSubtreeFilter = await ResolveDeptSubtreeFilterAsync(filter, cancellationToken);

        var queryable = ApplyUserListFilters(UserSet.AsNoTracking(), filter, deptSubtreeFilter, false, false);
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

        return list.Select(i => i with { Age = User.CalculateAge(i.BirthDate) }).ToList();
    }

    private async Task<IReadOnlyList<DeptId>?> ResolveDeptSubtreeFilterAsync(
        UserQueryInput query,
        CancellationToken cancellationToken)
    {
        if (query.OnlyMarketingCenterDeptSubtree)
            return await deptQuery.GetMarketingCenterSubtreeDeptIdsAsync(cancellationToken);
        if (query.OnlyTechnologyDeptSubtree)
            return await deptQuery.GetTechnologyDeptSubtreeDeptIdsAsync(cancellationToken);
        if (query.OnlyProductResearchCenterDeptSubtree)
            return await deptQuery.GetDutyAllowedDeptSubtreeIdsAsync(cancellationToken);
        return null;
    }

    private static IQueryable<User> ApplyUserListFilters(
        IQueryable<User> queryable,
        UserQueryInput query,
        IReadOnlyList<DeptId>? deptSubtreeFilter,
        bool omitFilterDeptNames,
        bool omitFilterRoleNames)
    {
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            queryable = queryable.Where(u => u.Name.Contains(query.Keyword!) || u.Email.Contains(query.Keyword!) || u.RealName.Contains(query.Keyword!));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(u => u.Status == query.Status);
        }

        if (query.IsResigned.HasValue)
        {
            queryable = queryable.Where(u => u.IsResigned == query.IsResigned.Value);
        }

        if (deptSubtreeFilter is not null)
        {
            if (deptSubtreeFilter.Count == 0)
                return queryable.Where(_ => false);
            queryable = queryable.Where(u => u.Dept != null && deptSubtreeFilter.Contains(u.Dept.DeptId));
        }
        else if (query.PositionId != null)
        {
            queryable = queryable.Where(u => u.Position != null && u.Position.PositionId == query.PositionId);
        }
        else if (query.DeptId != null)
        {
            queryable = queryable.Where(u => u.Dept != null && u.Dept.DeptId == query.DeptId);
        }

        if (query.OnlyProductResearchCenterDeptSubtree)
        {
            queryable = queryable.Where(u => u.AttendanceRequired);
        }

        if (!omitFilterDeptNames && query.FilterDeptNames is { Count: > 0 } deptNames)
        {
            var set = deptNames
                .Select(s => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim())
                .Distinct()
                .ToHashSet(StringComparer.Ordinal);
            if (set.Count > 0)
                queryable = queryable.Where(u =>
                    set.Contains(u.Dept != null ? u.Dept.DeptName : string.Empty));
        }

        if (!omitFilterRoleNames && query.FilterRoleNames is { Count: > 0 } roleNames)
        {
            var set = roleNames
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct()
                .ToHashSet(StringComparer.Ordinal);
            if (set.Count > 0)
                queryable = queryable.Where(u => u.Roles.Any(r => set.Contains(r.RoleName)));
        }

        return queryable;
    }

    /// <summary>
    /// 给定用户 ID 是否已离职或账号不存在（不存在视为已离职，用于订单业务经理展示等）。
    /// </summary>
    public async Task<Dictionary<UserId, bool>> GetUserIdToIsResignedMapAsync(
        IEnumerable<UserId> userIds,
        CancellationToken cancellationToken = default)
    {
        var list = (userIds ?? []).Where(u => u != UserId.Unassigned).Distinct().ToList();
        if (list.Count == 0)
            return new Dictionary<UserId, bool>();

        var found = await UserSet.AsNoTracking()
            .Where(u => list.Contains(u.Id))
            .Select(u => new { u.Id, u.IsResigned })
            .ToListAsync(cancellationToken);

        var map = list.ToDictionary(id => id, _ => true);
        foreach (var row in found)
            map[row.Id] = row.IsResigned;
        return map;
    }

    /// <summary>
    /// 含离职用户在内的展示名与部门（用于订单协作归属释放等）。
    /// </summary>
    public async Task<(string DisplayName, DeptId DeptId, string DeptName)?> GetUserDisplayAndDeptIncludingResignedAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var row = await UserSet.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.RealName,
                u.Name,
                DeptId = u.Dept != null ? u.Dept.DeptId : DeptId.Unassigned,
                DeptName = u.Dept != null ? u.Dept.DeptName : string.Empty,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (row == null)
            return null;
        var display = string.IsNullOrWhiteSpace(row.RealName) ? row.Name : row.RealName;
        return (display ?? string.Empty, row.DeptId, row.DeptName ?? string.Empty);
    }

    /// <summary>
    /// 批量解析用户展示名：优先真实姓名，其次员工档案姓名，最后登录账号。
    /// </summary>
    public async Task<IReadOnlyDictionary<UserId, string>> GetDisplayNamesByUserIdsAsync(
        IReadOnlyList<UserId> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<UserId, string>();
        }

        var users = await UserSet.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.RealName, u.Name })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<UserId, string>();
        foreach (var user in users)
        {
            var realName = user.RealName?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(realName) && !IsAsciiLoginName(realName))
            {
                result[user.Id] = realName;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(realName))
            {
                result[user.Id] = realName;
                continue;
            }

            result[user.Id] = user.Name?.Trim() ?? string.Empty;
        }

        return result;
    }

    /// <summary>
    /// 解析人事申请场景下的操作人展示名（优先用户真实姓名/员工档案，申请人本人可兜底快照姓名）。
    /// </summary>
    public static string ResolvePersonnelApplicationOperatorDisplayName(
        UserId operatorUserId,
        IReadOnlyDictionary<UserId, string> operatorNameById,
        UserId applicantId,
        string? applicantName)
    {
        if (operatorNameById.TryGetValue(operatorUserId, out var resolved)
            && !string.IsNullOrWhiteSpace(resolved)
            && !IsAsciiLoginName(resolved))
        {
            return resolved.Trim();
        }

        if (operatorUserId == applicantId
            && !string.IsNullOrWhiteSpace(applicantName))
        {
            return applicantName.Trim();
        }

        if (operatorNameById.TryGetValue(operatorUserId, out var fallback)
            && !string.IsNullOrWhiteSpace(fallback))
        {
            return fallback.Trim();
        }

        return string.Empty;
    }

    /// <summary>
    /// 解析人事申请场景下的操作人展示名（异步加载用户/员工档案姓名）。
    /// </summary>
    public async Task<string> ResolvePersonnelApplicationOperatorDisplayNameAsync(
        UserId operatorUserId,
        UserId applicantId,
        string? applicantName,
        CancellationToken cancellationToken = default)
    {
        var map = await GetDisplayNamesByUserIdsAsync([operatorUserId], cancellationToken);
        return ResolvePersonnelApplicationOperatorDisplayName(
            operatorUserId,
            map,
            applicantId,
            applicantName);
    }

    /// <summary>
    /// 解析单个用户展示名，可选申请人姓名快照兜底（与用户表/员工档案均为登录账号时）。
    /// </summary>
    public async Task<string> ResolveDisplayNameAsync(
        UserId userId,
        string? applicantNameFallback = null,
        CancellationToken cancellationToken = default)
    {
        var map = await GetDisplayNamesByUserIdsAsync([userId], cancellationToken);
        var resolved = map.TryGetValue(userId, out var name) ? name : string.Empty;
        if (!string.IsNullOrWhiteSpace(resolved) && !IsAsciiLoginName(resolved))
        {
            return resolved.Trim();
        }

        if (!string.IsNullOrWhiteSpace(applicantNameFallback))
        {
            return applicantNameFallback.Trim();
        }

        return resolved.Trim();
    }

    internal static bool IsAsciiLoginName(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.All(static c => c <= 127);

    private Expression<Func<User, UserInfoQueryDto>> ToUserInfoQueryDto()
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
            0,
            u.BirthDate,
            u.Dept != null ? u.Dept.DeptId : DeptId.Unassigned,
            u.Dept != null ? u.Dept.DeptName : string.Empty,
            u.Position != null ? u.Position.PositionId : (PositionId?)null,
            u.Position != null ? u.Position.PositionName : string.Empty,
            u.IdCardNumber,
            u.Address,
            u.Education,
            u.GraduateSchool,
            u.AvatarUrl,
            u.NotOrderMeal,
            u.OrderMealSort,
            u.AttendanceRequired,
            u.WechatGuid,
            u.IsResigned,
            u.ResignedTime,
            u.CreatorId,
            u.ModifierId,
            u.DeleterId,
            u.LastLoginTime,
            u.LastLoginIp,
            u.Dept != null && applicationDbContext.DeptResponsibleUsers
                .Any(r => r.DeptId == u.Dept.DeptId && r.UserId == u.Id),
            u.Dept != null && applicationDbContext.DeptResponsibleUsers
                .Any(r => r.DeptId == u.Dept.DeptId && r.UserId == u.Id && r.IsDefault));
    }

    /// <summary>
    /// 批量获取用户部门与角色（意见表、流程详情等展示处理人组织信息）。
    /// </summary>
    public async Task<IReadOnlyDictionary<UserId, UserDeptPositionSnapshot>> GetUserDeptPositionsByIdsAsync(
        IEnumerable<UserId> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds
            .Where(id => id != UserId.Unassigned)
            .Distinct()
            .ToList();
        if (ids.Count == 0)
            return new Dictionary<UserId, UserDeptPositionSnapshot>();

        var rows = await UserSet.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                DeptName = u.Dept != null ? u.Dept.DeptName : string.Empty,
                RoleNames = u.Roles.Select(r => r.RoleName).ToList(),
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.Id,
            r => new UserDeptPositionSnapshot(
                r.Id,
                r.DeptName,
                string.Join("、", r.RoleNames.Where(n => !string.IsNullOrWhiteSpace(n)))));
    }

    /// <summary>
    /// 构建流程定义导入时的用户 ID 重映射索引（按真实姓名/登录名）。
    /// </summary>
    public async Task<WorkflowRemapUserIndex> BuildWorkflowRemapUserIndexAsync(CancellationToken cancellationToken = default)
    {
        var rows = await UserSet.AsNoTracking()
            .Where(u => u.Status == 1 && u.IsActive && !u.IsResigned)
            .Select(u => new { u.Id, u.Name, u.RealName })
            .ToListAsync(cancellationToken);

        var index = new WorkflowRemapUserIndex();
        foreach (var row in rows)
        {
            var displayName = string.IsNullOrWhiteSpace(row.RealName) ? row.Name : row.RealName;
            index.Add(row.Id, displayName, row.Name);
        }

        return index;
    }

    /// <summary>
    /// 值日轮值人员：产品研发中心、网络推广组及其下级，在职已启用且参与考勤，按订餐排序号正序。
    /// </summary>
    public async Task<IReadOnlyList<DutyRotationUserDto>> GetDutyRotationUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var deptIds = await deptQuery.GetDutyAllowedDeptSubtreeIdsAsync(cancellationToken);
        return await GetDutyRotationUsersByDeptIdsAsync(deptIds, cancellationToken);
    }

    /// <summary>
    /// 值日轮值人员：指定部门及下级、在职已启用且参与考勤，按订餐排序号正序（与月订餐汇总一致）。
    /// </summary>
    public async Task<IReadOnlyList<DutyRotationUserDto>> GetDutyRotationUsersByDeptIdsAsync(
        IReadOnlyList<DeptId> deptIds,
        CancellationToken cancellationToken = default)
    {
        if (deptIds.Count == 0)
            return [];

        return await UserSet.AsNoTracking()
            .Where(u => !u.IsDeleted && !u.IsResigned && u.IsActive && u.Status == 1)
            .Where(u => u.AttendanceRequired)
            .Where(u => u.Dept != null && deptIds.Contains(u.Dept.DeptId))
            .OrderBy(u => u.OrderMealSort == 0 ? 1 : 0)
            .ThenBy(u => u.OrderMealSort)
            .ThenBy(u => u.CreatedAt)
            .ThenBy(u => u.Id)
            .Select(u => new DutyRotationUserDto(
                u.Id,
                u.Dept!.DeptId,
                string.IsNullOrWhiteSpace(u.RealName) ? u.Name : u.RealName,
                u.OrderMealSort))
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// 值日轮值人员（含排序号）。
/// </summary>
public record DutyRotationUserDto(UserId UserId, DeptId DeptId, string DisplayName, int OrderMealSort);

/// <summary>
/// 用户部门与角色快照（批量查询用）。
/// </summary>
public record UserDeptPositionSnapshot(UserId UserId, string DeptName, string RoleNames);

