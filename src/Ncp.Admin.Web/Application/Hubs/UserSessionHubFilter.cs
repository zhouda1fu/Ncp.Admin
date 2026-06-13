using Microsoft.AspNetCore.SignalR;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Application.Hubs;

public sealed class UserSessionHubFilter(IUserSessionService userSessionService) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var user = invocationContext.Context.User;
        var sessionId = user?.FindFirst(UserSessionClaimTypes.SessionId)?.Value;
        if (user is null
            || !user.TryGetUserId(out var userId)
            || string.IsNullOrWhiteSpace(sessionId)
            || !await userSessionService.IsCurrentAsync(userId.Id, sessionId))
        {
            throw new HubException("账号已在其他设备登录");
        }

        return await next(invocationContext);
    }
}
