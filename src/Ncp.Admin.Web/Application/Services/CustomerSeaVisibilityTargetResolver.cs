using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Services;

/// <summary>
/// 根据客户上的电话/项目区域与公海片区分配表，解析应对其可见的用户集合。
/// </summary>
public interface ICustomerSeaVisibilityTargetResolver
{
    Task<IReadOnlyList<UserId>> ResolveUserIdsAsync(Customer customer, CancellationToken cancellationToken = default);
}

public class CustomerSeaVisibilityTargetResolver(ApplicationDbContext dbContext) : ICustomerSeaVisibilityTargetResolver
{
    public async Task<IReadOnlyList<UserId>> ResolveUserIdsAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var customerRegionIds = CollectCustomerRegionIds(customer);
        if (customerRegionIds.Count == 0)
            return [];

        var parentById = await dbContext.Regions.AsNoTracking()
            .Select(r => new { r.Id, r.ParentId })
            .ToDictionaryAsync(x => x.Id, x => x.ParentId, cancellationToken);

        var assignments = await dbContext.CustomerSeaRegionAssignments.AsNoTracking()
            .Include(a => a.Regions)
            .ToListAsync(cancellationToken);

        var result = new HashSet<UserId>();
        foreach (var assignment in assignments)
        {
            foreach (var link in assignment.Regions)
            {
                foreach (var cNode in customerRegionIds)
                {
                    if (IsCustomerRegionUnderAssignmentRoot(link.RegionId, cNode, parentById))
                    {
                        result.Add(assignment.TargetUserId);
                        break;
                    }
                }
            }
        }

        return result.ToList();
    }

    private static HashSet<RegionId> CollectCustomerRegionIds(Customer customer)
    {
        var set = new HashSet<RegionId>();
        void AddCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return;
            if (long.TryParse(code.Trim(), out var v) && v != 0)
                set.Add(new RegionId(v));
        }

        AddCode(customer.PhoneProvinceCode);
        AddCode(customer.PhoneCityCode);
        AddCode(customer.PhoneDistrictCode);
        AddCode(customer.ProvinceCode);
        AddCode(customer.CityCode);
        AddCode(customer.DistrictCode);
        return set;
    }

    /// <summary>
    /// 客户区域节点 C 是否落在片区分配根 R 的子树内（R 为分配表中保存的区域，含展开后的末级）。
    /// </summary>
    private static bool IsCustomerRegionUnderAssignmentRoot(
        RegionId assignmentRegionId,
        RegionId customerRegionId,
        IReadOnlyDictionary<RegionId, RegionId> parentById)
    {
        var cur = customerRegionId;
        while (true)
        {
            if (cur == assignmentRegionId)
                return true;
            if (!parentById.TryGetValue(cur, out var parent) || parent == default(RegionId))
                return false;
            cur = parent;
        }
    }
}
