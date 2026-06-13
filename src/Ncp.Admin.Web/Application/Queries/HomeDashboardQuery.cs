using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Web.Application.Services.Dashboard;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>行事历月内便签摘要。</summary>
public record HomeDashboardCalendarMemoDayDto(DateOnly Date, bool HasContent);

/// <summary>行事历与便签。</summary>
public record HomeDashboardCalendarDto(
    DateOnly Today,
    int Year,
    int Month,
    DateOnly? BirthdayMonthDay,
    IReadOnlyList<HomeDashboardCalendarMemoDayDto> MemoDays);

/// <summary>后台首页工作台汇总（平台精简版）。</summary>
/// <param name="WorkflowPendingTaskCount">工作流待办任务数。</param>
/// <param name="UnreadNotificationCount">未读站内通知数。</param>
/// <param name="Calendar">行事历与便签摘要。</param>
/// <param name="CardOrder">首页卡片排序偏好。</param>
public record HomeDashboardDto(
    int WorkflowPendingTaskCount,
    int UnreadNotificationCount,
    HomeDashboardCalendarDto Calendar,
    IReadOnlyList<string> CardOrder);

/// <summary>
/// 当前登录用户的首页工作台汇总查询。
/// </summary>
public class HomeDashboardQuery(ApplicationDbContext dbContext, UserQuery userQuery) : IQuery
{
    private static readonly TimeZoneInfo ChinaTimeZone = ResolveChinaTimeZone();

    public async Task<HomeDashboardDto> GetForCurrentUserAsync(
        UserId userId,
        IReadOnlySet<string> grantedPermissionCodes,
        int? calendarYear = null,
        int? calendarMonth = null,
        CancellationToken cancellationToken = default)
    {
        var today = TodayInChina();
        var workflowPendingTaskCount = await CountWorkflowPendingTasksAsync(userId, cancellationToken);
        var unreadNotificationCount = await CountUnreadNotificationsAsync(userId, cancellationToken);
        var calendar = await BuildCalendarAsync(userId, today, calendarYear, calendarMonth, cancellationToken);
        var cardOrder = await BuildCardOrderAsync(userId, grantedPermissionCodes, cancellationToken);

        return new HomeDashboardDto(
            workflowPendingTaskCount,
            unreadNotificationCount,
            calendar,
            cardOrder);
    }

    private async Task<int> CountWorkflowPendingTasksAsync(UserId userId, CancellationToken cancellationToken)
    {
        var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(userId, cancellationToken);

        return await (
            from t in dbContext.WorkflowTasks.AsNoTracking()
            join s in dbContext.WorkflowTaskAssignmentSnapshots.AsNoTracking()
                on t.Id equals s.WorkflowTaskId
            join i in dbContext.WorkflowInstances.AsNoTracking().IgnoreQueryFilters()
                on t.WorkflowInstanceId equals i.Id
            where i.Status == WorkflowInstanceStatus.Running
                  && t.Status == WorkflowTaskStatus.Pending
                  && ((s.AssigneeType == AssigneeType.User && s.AssigneeUserId == userId)
                      || (s.AssigneeType == AssigneeType.Role && userRoleIds.Contains(s.AssigneeRoleId)))
            select t.Id)
            .CountAsync(cancellationToken);
    }

    private Task<int> CountUnreadNotificationsAsync(UserId userId, CancellationToken cancellationToken) =>
        dbContext.Notifications.AsNoTracking()
            .Where(n => n.ReceiverId == userId.Id && !n.IsRead && !n.IsDeleted)
            .CountAsync(cancellationToken);

    private async Task<HomeDashboardCalendarDto> BuildCalendarAsync(
        UserId userId,
        DateOnly today,
        int? calendarYear,
        int? calendarMonth,
        CancellationToken cancellationToken)
    {
        var year = calendarYear is >= 2000 and <= 2100 ? calendarYear.Value : today.Year;
        var month = calendarMonth is >= 1 and <= 12 ? calendarMonth.Value : today.Month;
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var memoDays = await dbContext.UserCalendarMemos.AsNoTracking()
            .Where(m => m.UserId == userId && m.MemoDate >= monthStart && m.MemoDate <= monthEnd)
            .Select(m => new HomeDashboardCalendarMemoDayDto(m.MemoDate, m.Content != ""))
            .ToListAsync(cancellationToken);

        DateOnly? birthdayMonthDay = null;
        var userBirth = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.BirthDate)
            .FirstOrDefaultAsync(cancellationToken);
        if (userBirth != DateTimeOffset.MinValue)
        {
            var b = userBirth.ToOffset(ChinaTimeZone.GetUtcOffset(userBirth));
            birthdayMonthDay = new DateOnly(b.Year, b.Month, b.Day);
        }

        return new HomeDashboardCalendarDto(today, year, month, birthdayMonthDay, memoDays);
    }

    private async Task<IReadOnlyList<string>> BuildCardOrderAsync(
        UserId userId,
        IReadOnlySet<string> grantedPermissionCodes,
        CancellationToken cancellationToken)
    {
        var pref = await dbContext.UserHomeDashboardPreferences.AsNoTracking()
            .Where(p => p.Id == userId)
            .Select(p => p.CardOrderJson)
            .FirstOrDefaultAsync(cancellationToken);

        IReadOnlyList<string> merged;
        if (string.IsNullOrWhiteSpace(pref))
        {
            merged = HomeDashboardCardKeys.DefaultCardOrder;
        }
        else
        {
            try
            {
                var order = JsonSerializer.Deserialize<List<string>>(pref);
                if (order is null || order.Count == 0)
                {
                    merged = HomeDashboardCardKeys.DefaultCardOrder;
                }
                else
                {
                    var known = new HashSet<string>(HomeDashboardCardKeys.DefaultCardOrder, StringComparer.Ordinal);
                    var sortableFromPref = order
                        .Where(k => known.Contains(k) && !HomeDashboardCardKeys.IsPinnedCard(k))
                        .ToList();
                    var sortableDefaults = HomeDashboardCardKeys.DefaultCardOrder
                        .Where(k => !HomeDashboardCardKeys.IsPinnedCard(k));
                    sortableFromPref.AddRange(sortableDefaults.Where(k => !sortableFromPref.Contains(k, StringComparer.Ordinal)));
                    var list = new List<string>(HomeDashboardCardKeys.PinnedCardOrder);
                    list.AddRange(sortableFromPref);
                    merged = list;
                }
            }
            catch (JsonException)
            {
                merged = HomeDashboardCardKeys.DefaultCardOrder;
            }
        }

        return HomeDashboardCardAccess.FilterVisibleCardKeys(merged, grantedPermissionCodes).ToList();
    }

    private static DateOnly TodayInChina() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ChinaTimeZone));

    private static TimeZoneInfo ResolveChinaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
        }
    }
}
