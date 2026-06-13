using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Application.Services.Dashboard;

/// <summary>首页卡片可见性与排序过滤（平台精简版）。</summary>
public static class HomeDashboardCardAccess
{
    public static bool CanViewCard(string cardKey, IReadOnlySet<string> grantedPermissionCodes) =>
        cardKey switch
        {
            HomeDashboardCardKeys.Process =>
                grantedPermissionCodes.Contains(PermissionCodes.HomeDashboard)
                || grantedPermissionCodes.Contains(PermissionCodes.WorkflowInstanceView),
            HomeDashboardCardKeys.Calendar => true,
            _ => false,
        };

    public static IEnumerable<string> FilterVisibleCardKeys(
        IReadOnlyList<string> keys,
        IReadOnlySet<string> grantedPermissionCodes) =>
        keys.Where(k => CanViewCard(k, grantedPermissionCodes));

    public static IEnumerable<string> FilterSortableCardKeysForSave(
        IReadOnlyList<string> cardOrder,
        IReadOnlySet<string> grantedPermissionCodes) =>
        cardOrder
            .Where(k => k == HomeDashboardCardKeys.Process && CanViewCard(k, grantedPermissionCodes))
            .Distinct(StringComparer.Ordinal);
}
