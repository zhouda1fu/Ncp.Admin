using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Ncp.Admin.Web.Application.Services.Notification;

public interface IWeChatAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    Task<string?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 获取并缓存微信公众号 access_token。
/// </summary>
public class WeChatAccessTokenProvider(
    IHttpClientFactory httpClientFactory,
    IMemoryCache memoryCache,
    IOptions<WeChatOfficialAccountOptions> options,
    ILogger<WeChatAccessTokenProvider> logger) : IWeChatAccessTokenProvider
{
    private const string CacheKey = "WeChatOfficialAccount:AccessToken";

    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        => GetAccessTokenCoreAsync(forceRefresh: false, cancellationToken);

    public Task<string?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(CacheKey);
        return GetAccessTokenCoreAsync(forceRefresh: true, cancellationToken);
    }

    private async Task<string?> GetAccessTokenCoreAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        var opt = options.Value;
        if (!opt.Enabled || string.IsNullOrWhiteSpace(opt.AppId) || string.IsNullOrWhiteSpace(opt.Secret))
        {
            return null;
        }

        if (!forceRefresh && memoryCache.TryGetValue<string>(CacheKey, out var cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
        {
            return cachedToken;
        }

        var client = httpClientFactory.CreateClient(WeChatOfficialAccountClient.HttpClientName);
        var response = await client.PostAsJsonAsync(
            "/cgi-bin/stable_token",
            new StableTokenRequest("client_credential", opt.AppId, opt.Secret, forceRefresh),
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<StableTokenResponse>(cancellationToken);
        if (!response.IsSuccessStatusCode || result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            logger.LogWarning(
                "获取微信公众号 access_token 失败，HTTP状态码：{StatusCode}，错误码：{ErrCode}，错误信息：{ErrMsg}",
                response.StatusCode,
                result?.ErrCode,
                result?.ErrMsg);
            return null;
        }

        var expiresIn = result.ExpiresIn > 300 ? result.ExpiresIn - 300 : 3600;
        memoryCache.Set(CacheKey, result.AccessToken, TimeSpan.FromSeconds(expiresIn));
        return result.AccessToken;
    }

    private sealed record StableTokenRequest(
        [property: JsonPropertyName("grant_type")] string GrantType,
        [property: JsonPropertyName("appid")] string AppId,
        [property: JsonPropertyName("secret")] string Secret,
        [property: JsonPropertyName("force_refresh")] bool ForceRefresh);

    private sealed record StableTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("errcode")] int ErrCode,
        [property: JsonPropertyName("errmsg")] string? ErrMsg);
}
