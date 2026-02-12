using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Ncp.Admin.Web.Application.Hubs;

/// <summary>
/// 使用 JWT 中的 NameIdentifier 作为 SignalR 连接的用户标识，以便服务端通过 Clients.User(userId) 推送
/// </summary>
public class NameUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
