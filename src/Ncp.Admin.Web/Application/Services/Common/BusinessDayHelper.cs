namespace Ncp.Admin.Web.Application.Services.Common;

/// <summary>
/// 工作日历辅助（工作日映射通常来自 <see cref="Attendance.ScheduleWorkDayService"/> 读排班表）。
/// </summary>
public static class BusinessDayHelper
{
    /// <summary>
    /// 从起始日（不含）起向后累计 <paramref name="businessDays"/> 个工作日，返回最后一个工作日的日期。
    /// </summary>
    public static DateOnly AddBusinessDaysAfter(
        DateOnly startDate,
        int businessDays,
        IReadOnlyDictionary<string, bool> workDayMap)
    {
        if (businessDays <= 0)
            return startDate;

        var current = startDate;
        var counted = 0;
        while (counted < businessDays)
        {
            current = current.AddDays(1);
            if (IsBusinessDay(current, workDayMap))
                counted++;
        }

        return current;
    }

    public static bool IsBusinessDay(DateOnly date, IReadOnlyDictionary<string, bool> workDayMap)
    {
        var key = date.ToString("yyyy-MM-dd");
        if (workDayMap.TryGetValue(key, out var isWork))
            return isWork;

        return date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
    }

    /// <summary>
    /// 统计当月从 1 日起至 <paramref name="date"/>（含）已过去的工作日数量。
    /// </summary>
    public static int CountBusinessDaysInMonthUpTo(
        DateOnly date,
        IReadOnlyDictionary<string, bool> workDayMap)
    {
        var monthStart = new DateOnly(date.Year, date.Month, 1);
        var count = 0;
        for (var current = monthStart; current <= date; current = current.AddDays(1))
        {
            if (IsBusinessDay(current, workDayMap))
                count++;
        }

        return count;
    }

    /// <summary>
    /// 流程申请可选的最早日期：每月前 <paramref name="graceBusinessDays"/> 个工作日允许选上月，之后仅允许当月及以后。
    /// </summary>
    public static DateOnly GetEarliestAllowedApplicationDate(
        DateOnly today,
        int graceBusinessDays,
        IReadOnlyDictionary<string, bool> workDayMap)
    {
        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);
        if (graceBusinessDays <= 0)
            return currentMonthStart;

        var businessDaysElapsed = CountBusinessDaysInMonthUpTo(today, workDayMap);
        if (businessDaysElapsed <= graceBusinessDays)
            return currentMonthStart.AddMonths(-1);

        return currentMonthStart;
    }
}
