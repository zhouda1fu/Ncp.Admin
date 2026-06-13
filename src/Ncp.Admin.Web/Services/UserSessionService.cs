using StackExchange.Redis;

namespace Ncp.Admin.Web.Services;

public static class UserSessionClaimTypes
{
    public const string SessionId = "sid";
}

public static class UserSessionAuthenticationReasons
{
    public const string HeaderName = "X-Auth-Reason";
    public const string SessionReplaced = "session-replaced";
    public const string SessionStoreUnavailable = "session-store-unavailable";
}

public interface IUserSessionService
{
    Task ReplaceAsync(long userId, string sessionId, TimeSpan lifetime);
    Task<bool> IsCurrentAsync(long userId, string sessionId);
    Task<bool> RemoveIfCurrentAsync(long userId, string sessionId);
}

public sealed class UserSessionService(IConnectionMultiplexer connectionMultiplexer) : IUserSessionService
{
    private const string KeyPrefix = "auth:session:";

    private static readonly LuaScript RemoveIfCurrentScript = LuaScript.Prepare(
        """
        if redis.call('get', @key) == @sessionId then
            return redis.call('del', @key)
        end
        return 0
        """);

    public Task ReplaceAsync(long userId, string sessionId, TimeSpan lifetime) =>
        connectionMultiplexer.GetDatabase().StringSetAsync(GetKey(userId), sessionId, lifetime);

    public async Task<bool> IsCurrentAsync(long userId, string sessionId)
    {
        var currentSessionId = await connectionMultiplexer.GetDatabase().StringGetAsync(GetKey(userId));
        return currentSessionId.HasValue
               && string.Equals(currentSessionId.ToString(), sessionId, StringComparison.Ordinal);
    }

    public async Task<bool> RemoveIfCurrentAsync(long userId, string sessionId)
    {
        var result = await connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(
            RemoveIfCurrentScript,
            new
            {
                key = (RedisKey)GetKey(userId),
                sessionId = (RedisValue)sessionId
            });
        return (long)result > 0;
    }

    private static string GetKey(long userId) => $"{KeyPrefix}{userId}";
}
