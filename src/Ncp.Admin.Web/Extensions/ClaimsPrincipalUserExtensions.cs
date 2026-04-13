using System.Security.Claims;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Web.Extensions;

/// <summary>
/// 从 JWT Claims 解析当前登录用户（端点层复用，避免重复调用 ClaimsPrincipal.FindFirstValue 与 TryParse）。
/// </summary>
public static class ClaimsPrincipalUserExtensions
{
    /// <summary>
    /// 解析 <c>NameIdentifier</c>（long）为 <see cref="UserId"/>。
    /// </summary>
    public static bool TryGetUserId(this ClaimsPrincipal? user, out UserId userId)
    {
        var s = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(s) || !long.TryParse(s, out var id))
        {
            userId = default!;
            return false;
        }

        userId = new UserId(id);
        return true;
    }

    /// <summary>
    /// 对应 <c>Name</c> claim；缺失时为空串。
    /// </summary>
    public static string GetUserDisplayName(this ClaimsPrincipal? user) =>
        user?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    /// <summary>
    /// 优先 <c>Name</c>，否则 <c>GivenName</c>（与操作日志等展示一致）。
    /// </summary>
    public static string GetUserDisplayNameOrGivenName(this ClaimsPrincipal? user)
    {
        var name = user?.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrEmpty(name))
            return name;
        return user?.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
    }

    /// <summary>
    /// 解析当前用户 ID；未登录或 claim 无效时返回 <c>null</c>（匿名/可选场景）。
    /// </summary>
    public static UserId? GetUserIdOrNull(this ClaimsPrincipal? user)
    {
        if (user is null)
            return null;
        return user.TryGetUserId(out var id) ? id : null;
    }
}
