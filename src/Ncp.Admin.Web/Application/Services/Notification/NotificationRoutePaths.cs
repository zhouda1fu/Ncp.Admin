using Ncp.Admin.Web.Application.Commands.Workflows;

namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 通知业务跳转路径（平台精简：工作流 + 系统通知）。
/// </summary>
public static class NotificationRoutePaths
{
    public static NotificationNavigationResult Home() => new("/");

    public static NotificationNavigationResult FromBusinessType(string businessType, string? businessId) =>
        businessType switch
        {
            "WorkflowInstance" when !string.IsNullOrWhiteSpace(businessId) =>
                WorkflowInstance(businessId),
            "UserFeedback" when !string.IsNullOrWhiteSpace(businessId) =>
                new("/system/user-feedback", NotificationDetailQuery(businessId)),
            "UserFeedback" => new("/system/user-feedback"),
            _ => Home(),
        };

    public static NotificationNavigationResult WorkflowInstance(string instanceId) =>
        new($"/workflow/instance/{instanceId}");

    private static Dictionary<string, string> NotificationDetailQuery(string id) =>
        new()
        {
            ["id"] = id,
            ["from"] = "notification",
        };
}

/// <summary>
/// 站内通知跳转路径与可选查询参数。
/// </summary>
/// <param name="Path">前端路由 path。</param>
/// <param name="Query">可选 query 参数。</param>
public record NotificationNavigationResult(
    string Path,
    IReadOnlyDictionary<string, string>? Query = null);
