using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ncp.Admin.Web.Application.Hubs;

/// <summary>
/// 通知推送 Hub，前端连接后服务端通过 IHubContext 向指定用户推送站内通知。
/// 需配置 IUserIdProvider 使 UserIdentifier 为用户 ID，以便服务端使用 Clients.User(userId).
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
}
