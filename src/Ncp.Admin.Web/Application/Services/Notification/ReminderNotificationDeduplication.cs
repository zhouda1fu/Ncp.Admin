using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 定时提醒通知：按 BusinessType + BusinessId + 接收人同日防重复。
/// </summary>
internal static class ReminderNotificationDeduplication
{
    public static async Task<HashSet<long>> GetAlreadySentReceiverIdsAsync(
        ApplicationDbContext db,
        string businessType,
        string businessId,
        CancellationToken cancellationToken)
    {
        return (await db.Notifications.AsNoTracking()
            .Where(n =>
                n.BusinessType == businessType
                && n.BusinessId == businessId
                && !n.IsDeleted)
            .Select(n => n.ReceiverId)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();
    }
}
