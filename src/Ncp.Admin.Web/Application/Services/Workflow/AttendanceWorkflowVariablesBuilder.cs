using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Web.Application.Services.Workflow;

public class AttendanceWorkflowVariables
{
    public string WorkflowId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RecordDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Days { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
}

/// <summary>
/// 考勤工作流变量构造器，统一 Variables 字段格式。
/// </summary>
public static class AttendanceWorkflowVariablesBuilder
{
    public static AttendanceWorkflowVariables Build(
        string workflowId,
        UserId userId,
        DateOnly recordDate,
        string type,
        decimal days,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null)
    {
        return new AttendanceWorkflowVariables
        {
            WorkflowId = workflowId,
            UserId = userId.ToString(),
            RecordDate = recordDate.ToString("yyyy-MM-dd"),
            Type = type,
            Days = days,
            StartTime = startTime?.ToString("O"),
            EndTime = endTime?.ToString("O"),
        };
    }

    public static string BuildJson(
        string workflowId,
        UserId userId,
        DateOnly recordDate,
        string type,
        decimal days,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null)
    {
        return JsonSerializer.Serialize(Build(workflowId, userId, recordDate, type, days, startTime, endTime));
    }
}
