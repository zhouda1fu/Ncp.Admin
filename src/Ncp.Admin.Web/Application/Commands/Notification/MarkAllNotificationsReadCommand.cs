using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Notification;

/// <summary>
/// 批量标记通知为已读命令
/// </summary>
public record MarkAllNotificationsReadCommand(long ReceiverId) : ICommand<int>;

/// <summary>
/// 批量标记通知为已读命令处理器
/// </summary>
public class MarkAllNotificationsReadCommandHandler(ApplicationDbContext dbContext) : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    public async Task<int> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var count = await dbContext.Notifications
            .Where(n => n.ReceiverId == request.ReceiverId && !n.IsRead && !n.IsDeleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now),
            cancellationToken);

        return count;
    }
}
