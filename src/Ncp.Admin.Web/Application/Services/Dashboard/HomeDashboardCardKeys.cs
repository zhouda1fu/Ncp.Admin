namespace Ncp.Admin.Web.Application.Services.Dashboard;

/// <summary>平台首页可配置卡片 key。</summary>
public static class HomeDashboardCardKeys
{
    public const string Process = "process";
    public const string Calendar = "calendar";

    public static readonly IReadOnlyList<string> DefaultCardOrder = [Process, Calendar];

    public static readonly IReadOnlyList<string> PinnedCardOrder = [Calendar];

    public static bool IsKnownCardKey(string? key) =>
        key is Process or Calendar;

    public static bool IsPinnedCard(string key) => key == Calendar;
}
