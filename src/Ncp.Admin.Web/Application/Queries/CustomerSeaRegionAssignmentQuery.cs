using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Services;
using NetCorePal.Context;
using System.Text;

namespace Ncp.Admin.Web.Application.Queries;

public record CustomerSeaRegionAssignUserListItemDto(
    UserId UserId,
    string DeptName,
    IReadOnlyList<string> RoleNames,
    string Name);

public record CustomerSeaRegionAssignAuditListItemDto(
    CustomerSeaRegionAssignmentAuditId Id,
    string OperatorUserName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> AddedRegionNames,
    IReadOnlyList<string> RemovedRegionNames);

public record CustomerSeaRegionAssignAuthorizedRegionsSummaryDto(
    int AuthorizedUserCount,
    bool IsTargetVisible,
    IReadOnlyList<RegionId> AuthorizedRegionIds,
    IReadOnlyList<string> AuthorizedRegionNames,
    IReadOnlyList<RegionId> TargetRegionIds,
    IReadOnlyList<string> TargetRegionNames,
    IReadOnlyList<RegionId> MissingFromTargetRegionIds,
    IReadOnlyList<string> MissingFromTargetRegionNames);

public class CustomerSeaRegionAssignUsersQueryInput : PageRequest
{
    public string? Keyword { get; set; }
}

public class CustomerSeaRegionAssignAuditsQueryInput : PageRequest
{
}

/// <summary>
/// 客户公海片区分配查询
/// </summary>
public class CustomerSeaRegionAssignmentQuery(
    ApplicationDbContext dbContext,
    DeptQuery deptQuery,
    RoleQuery roleQuery,
    IContextAccessor contextAccessor)
    : IQuery
{
    /// <summary>
    /// 临时调试日志：用于排查“数据权限范围与展示片区范围不一致”的问题。
    /// 日志写入系统临时目录，便于你复现后我直接读取分析。
    /// </summary>
    private static void DebugLog(string content)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "sea-region-assign-permission-debug.log");
            File.AppendAllText(path, content + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// 公海片区分配：按<strong>被分配人</strong>在库中的角色数据权限，计算其在「营销中心」内可关联的可见人员（片区并集以此为准）。
    /// 与当前登录操作者 JWT 无关，避免操作者角色更宽/更窄导致回显与业务预期不一致。
    /// 多角色时：若全部为 All 则营销中心内全员；否则合并非 All 角色的部门范围（与登录时 Custom/DeptAndSub 展开方式一致）。
    /// </summary>
    private async Task<List<UserId>> GetSeaRegionAuthorizedMarketingUserIdsAsync(
        UserId assigneeUserId,
        IReadOnlyList<DeptId> marketingCenterDeptIds,
        CancellationToken cancellationToken)
    {
        var roleIds = await dbContext.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == assigneeUserId)
            .Select(ur => ur.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            DebugLog($"GetSeaRegionAuthorizedMarketingUserIds: assigneeUserId={assigneeUserId.Id}, roleIdsCount=0 -> []");
            return [];
        }

        var adminRoles = await roleQuery.GetAdminRolesForAssignmentAsync(roleIds, cancellationToken);
        if (adminRoles.Count == 0)
        {
            DebugLog($"GetSeaRegionAuthorizedMarketingUserIds: assigneeUserId={assigneeUserId.Id}, adminRolesCount=0 -> []");
            return [];
        }

        // 仅当「所有」角色都是 All 时，才在营销中心内放开为全员（与纯超级数据权限一致）
        if (adminRoles.TrueForAll(r => r.DataScope == DataScope.All))
        {
            var allInMc = await dbContext.Users.AsNoTracking()
                .Where(u => !u.IsResigned)
                .Where(u => u.Dept != null && marketingCenterDeptIds.Contains(u.Dept.DeptId))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            DebugLog(
                $"GetSeaRegionAuthorizedMarketingUserIds: assigneeUserId={assigneeUserId.Id}, mode=allRolesAll, count={allInMc.Count}");
            return allInMc;
        }

        var assigneeDeptRow = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == assigneeUserId && u.Dept != null)
            .Select(u => new { u.Dept!.DeptId })
            .FirstOrDefaultAsync(cancellationToken);

        var explicitUserIds = new HashSet<UserId>();
        var allowedDeptIds = new HashSet<DeptId>();

        foreach (var role in adminRoles.Where(r => r.DataScope != DataScope.All))
        {
            switch (role.DataScope)
            {
                case DataScope.Self:
                    explicitUserIds.Add(assigneeUserId);
                    break;
                case DataScope.Dept:
                    if (assigneeDeptRow != null)
                        allowedDeptIds.Add(assigneeDeptRow.DeptId);
                    break;
                case DataScope.DeptAndSub:
                    if (assigneeDeptRow != null)
                    {
                        foreach (var id in await deptQuery.GetAllChildDeptIdsAsync(assigneeDeptRow.DeptId, cancellationToken))
                            allowedDeptIds.Add(id);
                    }
                    break;
                case DataScope.CustomDeptAndSub:
                    foreach (var root in role.CustomDeptIds)
                    {
                        foreach (var id in await deptQuery.GetAllChildDeptIdsAsync(root, cancellationToken))
                            allowedDeptIds.Add(id);
                    }
                    break;
            }
        }

        var allowedDeptList = allowedDeptIds.ToList();
        var explicitList = explicitUserIds.ToList();

        if (allowedDeptList.Count == 0 && explicitList.Count == 0)
        {
            DebugLog(
                $"GetSeaRegionAuthorizedMarketingUserIds: assigneeUserId={assigneeUserId.Id}, mode=nonAllButNoDeptOrSelf -> []");
            return [];
        }

        var result = await dbContext.Users.AsNoTracking()
            .Where(u => !u.IsResigned)
            .Where(u => u.Dept != null && marketingCenterDeptIds.Contains(u.Dept.DeptId))
            .Where(u => explicitList.Contains(u.Id) || allowedDeptList.Contains(u.Dept.DeptId))
            .Select(u => u.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        DebugLog(
            $"GetSeaRegionAuthorizedMarketingUserIds: assigneeUserId={assigneeUserId.Id}, mode=mergedNonAll, explicitUsers={explicitList.Count}, allowedDepts={allowedDeptList.Count}, resultCount={result.Count}, roleScopes=[{string.Join(",", adminRoles.Select(r => r.DataScope.ToString()).Distinct())}]");

        return result;
    }

    public async Task<PagedData<CustomerSeaRegionAssignUserListItemDto>> GetAssignUsersPagedAsync(
        CustomerSeaRegionAssignUsersQueryInput input,
        CancellationToken cancellationToken = default)
    {
        var marketingCenter = await deptQuery.GetDeptByExactNameAsync("营销中心", cancellationToken);
        if (marketingCenter is null)
            throw new KnownException("未找到营销中心部门，请检查部门名称配置/数据", ErrorCodes.CustomerSeaRegionAssignMarketingCenterDeptNotFound);

        var marketingCenterDeptIds = await deptQuery.GetAllChildDeptIdsAsync(marketingCenter.Id, cancellationToken);
        if (marketingCenterDeptIds.Count == 0)
            return new PagedData<CustomerSeaRegionAssignUserListItemDto>([], 0, input.PageIndex, input.PageSize);

        var query = dbContext.Users.AsNoTracking()
            .Where(u => !u.IsResigned)
            .Where(u => u.Dept != null && marketingCenterDeptIds.Contains(u.Dept.DeptId));

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var kw = input.Keyword.Trim();
            query = query.Where(u => (u.RealName ?? u.Name).Contains(kw) || u.Dept!.DeptName.Contains(kw));
        }

        var page = await query
            .OrderBy(u => u.Name)
            .Select(u => new CustomerSeaRegionAssignUserListItemDto(
                u.Id,
                u.Dept!.DeptName,
                u.Roles.Select(r => r.RoleName).Distinct().ToList(),
                u.RealName ?? u.Name))
            .ToPagedDataAsync(input, cancellationToken);

        return page;
    }

    public async Task<IReadOnlyList<RegionId>> GetUserRegionsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var dataPermission = contextAccessor.GetContext<DataPermissionContext>();
        if (dataPermission is null)
            return [];

        var marketingCenter = await deptQuery.GetDeptByExactNameAsync("营销中心", cancellationToken);
        if (marketingCenter is null)
            return [];

        var marketingCenterDeptIds = await deptQuery.GetAllChildDeptIdsAsync(marketingCenter.Id, cancellationToken);
        if (marketingCenterDeptIds.Count == 0)
            return [];

        var target = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId && !u.IsResigned && u.Dept != null)
            .Select(u => new { u.Dept!.DeptId })
            .FirstOrDefaultAsync(cancellationToken);

        if (target is null || !marketingCenterDeptIds.Contains(target.DeptId))
        {
            DebugLog($"GetUserRegionsAsync: userId={userId.Id}, targetDeptVisible=false");
            return [];
        }

        // 片区回显范围：以<strong>被查询用户本人</strong>的角色数据权限为准（与操作者无关）
        var visibleUserIds = await GetSeaRegionAuthorizedMarketingUserIdsAsync(
            userId,
            marketingCenterDeptIds,
            cancellationToken);

        if (!visibleUserIds.Contains(userId))
        {
            DebugLog($"GetUserRegionsAsync: userId={userId.Id}, targetDeptId={target.DeptId.Id}, notInAssigneeVisibleSet=true");
            return [];
        }

        DebugLog($"GetUserRegionsAsync: userId={userId.Id}, targetDeptId={target.DeptId.Id}, ok");
        return await dbContext.CustomerSeaRegionAssignments.AsNoTracking()
            .Where(x => x.TargetUserId == userId)
            .SelectMany(x => x.Regions.Select(r => r.RegionId))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedData<CustomerSeaRegionAssignAuditListItemDto>> GetUserAuditsPagedAsync(
        UserId userId,
        CustomerSeaRegionAssignAuditsQueryInput input,
        CancellationToken cancellationToken = default)
    {
        var dataPermission = contextAccessor.GetContext<DataPermissionContext>();
        if (dataPermission is null)
            return new PagedData<CustomerSeaRegionAssignAuditListItemDto>([], 0, input.PageIndex, input.PageSize);

        var marketingCenter = await deptQuery.GetDeptByExactNameAsync("营销中心", cancellationToken);
        if (marketingCenter is null)
            return new PagedData<CustomerSeaRegionAssignAuditListItemDto>([], 0, input.PageIndex, input.PageSize);

        var marketingCenterDeptIds = await deptQuery.GetAllChildDeptIdsAsync(marketingCenter.Id, cancellationToken);
        if (marketingCenterDeptIds.Count == 0)
            return new PagedData<CustomerSeaRegionAssignAuditListItemDto>([], 0, input.PageIndex, input.PageSize);

        var target = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId && !u.IsResigned && u.Dept != null)
            .Select(u => new { u.Dept!.DeptId })
            .FirstOrDefaultAsync(cancellationToken);

        if (target is null || !marketingCenterDeptIds.Contains(target.DeptId))
        {
            DebugLog($"GetUserAuditsPagedAsync: userId={userId.Id}, targetDeptVisible=false");
            return new PagedData<CustomerSeaRegionAssignAuditListItemDto>([], 0, input.PageIndex, input.PageSize);
        }

        var visibleUserIdsForAudits = await GetSeaRegionAuthorizedMarketingUserIdsAsync(
            userId,
            marketingCenterDeptIds,
            cancellationToken);

        if (!visibleUserIdsForAudits.Contains(userId))
        {
            DebugLog($"GetUserAuditsPagedAsync: userId={userId.Id}, targetDeptId={target.DeptId.Id}, notInAssigneeVisibleSet=true");
            return new PagedData<CustomerSeaRegionAssignAuditListItemDto>([], 0, input.PageIndex, input.PageSize);
        }

        var query = dbContext.CustomerSeaRegionAssignmentAudits.AsNoTracking()
            .Where(a => a.TargetUserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new CustomerSeaRegionAssignAuditListItemDto(
                a.Id,
                a.OperatorUserName,
                a.CreatedAt,
                a.Details
                    .Where(d => d.ChangeType == CustomerSeaRegionAssignmentAuditChangeType.Added)
                    .Select(d => d.RegionNameSnapshot)
                    .ToList(),
                a.Details
                    .Where(d => d.ChangeType == CustomerSeaRegionAssignmentAuditChangeType.Removed)
                    .Select(d => d.RegionNameSnapshot)
                    .ToList()));

        return await query.ToPagedDataAsync(input, cancellationToken);
    }

    /// <summary>
    /// 当有数据权限时：返回“授权范围内其他人员的已分配片区”与“当前人员已分配片区”的差异，
    /// 用于提示当前人员保存后可能发生的新增/变动。
    /// </summary>
    public async Task<CustomerSeaRegionAssignAuthorizedRegionsSummaryDto> GetAuthorizedRegionsSummaryAsync(
        UserId targetUserId,
        CancellationToken cancellationToken = default)
    {
        var ctx = contextAccessor.GetContext<DataPermissionContext>();

        var marketingCenter = await deptQuery.GetDeptByExactNameAsync("营销中心", cancellationToken)
            ?? throw new KnownException("未找到营销中心部门，请检查部门名称配置/数据", ErrorCodes.CustomerSeaRegionAssignMarketingCenterDeptNotFound);

        var marketingCenterDeptIds = await deptQuery.GetAllChildDeptIdsAsync(marketingCenter.Id, cancellationToken);
        if (ctx is null || marketingCenterDeptIds.Count == 0)
            return new CustomerSeaRegionAssignAuthorizedRegionsSummaryDto(
                AuthorizedUserCount: 0,
                IsTargetVisible: false,
                AuthorizedRegionIds: [],
                AuthorizedRegionNames: [],
                TargetRegionIds: [],
                TargetRegionNames: [],
                MissingFromTargetRegionIds: [],
                MissingFromTargetRegionNames: []);

        // 先解析目标用户是否属于“营销中心范围”（只限制片区模块，避免跨营销中心越权）
        var target = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == targetUserId && !u.IsResigned && u.Dept != null)
            .Select(u => new { u.Dept!.DeptId })
            .FirstOrDefaultAsync(cancellationToken);

        if (target is null || !marketingCenterDeptIds.Contains(target.DeptId))
            return new CustomerSeaRegionAssignAuthorizedRegionsSummaryDto(
                AuthorizedUserCount: 0,
                IsTargetVisible: false,
                AuthorizedRegionIds: [],
                AuthorizedRegionNames: [],
                TargetRegionIds: [],
                TargetRegionNames: [],
                MissingFromTargetRegionIds: [],
                MissingFromTargetRegionNames: []);

        // 授权人员范围：按<strong>被分配人</strong>在库中的角色数据权限合并（与当前登录者无关）
        var authorizedUserIds = await GetSeaRegionAuthorizedMarketingUserIdsAsync(
            targetUserId,
            marketingCenterDeptIds,
            cancellationToken);

        // 如果“目标人员”的某个角色数据范围为 Self（仅本人），
        // 则对该人员进行片区分配时，不应把同部门/同授权范围内其他人的片区并入“授权并集”，
        // 避免出现“仅本人”的业务经理被其他业务经理片区影响的情况。
        var targetHasSelfScope = await dbContext.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == targetUserId)
            .Join(
                dbContext.Roles.AsNoTracking(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.DataScope)
            .AnyAsync(scope => scope == DataScope.Self, cancellationToken);

        if (targetHasSelfScope)
        {
            authorizedUserIds = [targetUserId];
        }

        var isTargetVisible = authorizedUserIds.Contains(targetUserId);

        // 地区映射（用于把 RegionId -> RegionName）
        var regionMap = await dbContext.Regions.AsNoTracking()
            .Select(r => new { r.Id, r.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        // 授权范围内所有“片区”（Union）
        var authorizedRegionIds = await dbContext.CustomerSeaRegionAssignments.AsNoTracking()
            .Where(a => authorizedUserIds.Contains(a.TargetUserId))
            .SelectMany(a => a.Regions.Select(r => r.RegionId))
            .Distinct()
            .ToListAsync(cancellationToken);

        // 当前目标用户所有“片区”（仅在可见时返回）
        var targetRegionIdsAll = await dbContext.CustomerSeaRegionAssignments.AsNoTracking()
            .Where(a => a.TargetUserId == targetUserId)
            .SelectMany(a => a.Regions.Select(r => r.RegionId))
            .Distinct()
            .ToListAsync(cancellationToken);

        var targetRegionIds = isTargetVisible ? targetRegionIdsAll : [];
        var missingFromTargetRegionIds = isTargetVisible
            ? authorizedRegionIds.Except(targetRegionIdsAll).ToList()
            : [];

        IReadOnlyList<string> ToNames(IEnumerable<RegionId> ids) =>
            ids.Select(id => regionMap.TryGetValue(id, out var name) ? name : string.Empty)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

        return new CustomerSeaRegionAssignAuthorizedRegionsSummaryDto(
            AuthorizedUserCount: authorizedUserIds.Count,
            IsTargetVisible: isTargetVisible,
            AuthorizedRegionIds: authorizedRegionIds,
            AuthorizedRegionNames: ToNames(authorizedRegionIds),
            TargetRegionIds: targetRegionIds,
            TargetRegionNames: ToNames(targetRegionIds),
            MissingFromTargetRegionIds: missingFromTargetRegionIds,
            MissingFromTargetRegionNames: ToNames(missingFromTargetRegionIds));
    }
}

