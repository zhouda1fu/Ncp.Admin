using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Services.Dashboard;

namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 将站内通知归类到首页提醒卡片（平台精简：工作流 + 系统通知）。
/// </summary>
public static class NotificationHomeCardClassifier
{
    public static string Classify(
        NotificationType notificationType,
        string? businessType,
        string? workflowInstanceBusinessType = null)
    {
        if (string.Equals(businessType, "WorkflowInstance", StringComparison.Ordinal)
            || string.Equals(workflowInstanceBusinessType, WorkflowBusinessTypes.CreateUser, StringComparison.Ordinal))
        {
            return HomeDashboardCardKeys.Process;
        }

        if (string.Equals(businessType, "UserFeedback", StringComparison.Ordinal))
        {
            return HomeDashboardCardKeys.Process;
        }

        return HomeDashboardCardKeys.Process;
    }

    /// <summary>平台精简版无模块统计条。</summary>
    public static string? ClassifyModuleStatKey(
        NotificationType notificationType,
        string? businessType,
        string? workflowInstanceBusinessType) =>
        null;
}
