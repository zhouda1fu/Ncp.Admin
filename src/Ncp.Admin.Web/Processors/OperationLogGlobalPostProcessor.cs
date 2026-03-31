using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using FastEndpoints;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Web.Middleware;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Processors;

/// <summary>
/// 全局 PostProcessor：端点执行完成后记录写操作到 Channel（默认记录，基于 FastEndpoints 元数据生成模块/描述/类型）。
/// </summary>
public sealed class OperationLogGlobalPostProcessor : IGlobalPostProcessor
{
    private static readonly HashSet<string> ExcludedPathContains = new(StringComparer.OrdinalIgnoreCase)
    {
        "/login",
        "/logout",
        "/auth/",
        "operation-logs",
    };

    // 统一维护 Swagger Tag -> 业务模块名（用于操作日志展示）
    // 说明：不强制要求 Tags 本身是中文；Tags 仍可保持英文/技术分组。
    private static readonly IReadOnlyDictionary<string, string> TagToModuleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Users"] = "用户管理",
        ["Roles"] = "角色管理",
        ["Depts"] = "部门管理",
        ["Positions"] = "岗位管理",
        ["OperationLog"] = "操作日志",
        ["Workflow"] = "工作流管理",
        ["Customer"] = "客户管理",
        ["Order"] = "订单管理",
        ["Contract"] = "合同管理",
        ["Asset"] = "资产管理",
        ["Meeting"] = "会议管理",
        ["Attendance"] = "考勤管理",
        ["Announcement"] = "公告管理",
        ["Leave"] = "请假管理",
        ["Expense"] = "费用管理",
        ["Document"] = "文档管理",
        ["Vehicle"] = "车辆管理",
        ["Task"] = "任务管理",
        ["Contact"] = "联系人管理",
        ["Product"] = "产品管理",
    };

    public async Task PostProcessAsync(IPostProcessorContext ctx, CancellationToken ct)
    {
        var http = ctx.HttpContext;
        var req = http.Request;

        if (HttpMethods.IsGet(req.Method))
            return;
        var path = req.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            return;
        if (!http.User.Identity?.IsAuthenticated ?? true)
            return;
        foreach (var key in ExcludedPathContains)
        {
            if (path.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                return;
        }
        var endpoint = http.GetEndpoint();

        var userIdStr = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = http.User.FindFirstValue(ClaimTypes.Name) ?? http.User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userId))
            return;

        var statusCode = http.Response.StatusCode;
        var ip = http.Connection.RemoteIpAddress?.ToString();
        var userAgent = http.Request.Headers.UserAgent.FirstOrDefault();

        // 计算耗时：如果端点没有启用处理器状态，这里退化为 0；不影响日志主功能
        var durationMs = 0L;
        if (http.Items.TryGetValue("__oplog_sw", out var swObj) && swObj is Stopwatch sw)
            durationMs = sw.ElapsedMilliseconds;

        var requestBody = OperationLogPayloadSerializer.SerializeMasked(ctx.Request, 4000);
        var responseBody = OperationLogPayloadSerializer.SerializeMasked(ctx.Response, 4000);

        // 默认模块/类型（不依赖路径映射表）
        var module = GetModuleName(endpoint) ?? "API";
        var operationType = InferOperationType(path, req.Method);

        var entry = new OperationLogEntry(
            userId,
            userName,
            Trunc(module, 64),
            (int)operationType,
            path,
            req.Method,
            statusCode,
            statusCode is >= 200 and < 300,
            ip,
            userAgent,
            requestBody,
            responseBody,
            durationMs,
            DateTimeOffset.UtcNow);

        http.RequestServices.GetService<OperationLogChannel>()?.Write(entry);

        await Task.CompletedTask;
    }

    private static string? GetModuleName(Endpoint? endpoint)
    {
        // Try FastEndpoints EndpointDefinition tags if available
        var def = endpoint?.Metadata?.FirstOrDefault(m => string.Equals(m.GetType().FullName, "FastEndpoints.EndpointDefinition", StringComparison.Ordinal));
        if (def != null)
        {
            var t = def.GetType();
            var tagProp = t.GetProperty("Tags", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         ?? t.GetProperty("EndpointTags", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (tagProp != null)
            {
                var tagsObj = tagProp.GetValue(def);
                if (tagsObj is IEnumerable<string> tags)
                {
                    var first = tags.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(first))
                        return TagToModuleMap.TryGetValue(first, out var mapped) ? mapped : first;
                }
            }
        }
        return endpoint?.DisplayName;
    }

    private static OperationLogType InferOperationType(string path, string method)
    {
        var p = path.ToLowerInvariant();
        if (p.Contains("approve", StringComparison.Ordinal) || p.Contains("reject", StringComparison.Ordinal))
            return OperationLogType.Approve;
        if (p.Contains("submit", StringComparison.Ordinal))
            return OperationLogType.Submit;
        if (p.Contains("delete", StringComparison.Ordinal) || HttpMethods.IsDelete(method))
            return OperationLogType.Delete;
        if (HttpMethods.IsPut(method) || p.Contains("update", StringComparison.Ordinal) || p.Contains("edit", StringComparison.Ordinal))
            return OperationLogType.Update;
        if (HttpMethods.IsPost(method) || p.Contains("create", StringComparison.Ordinal) || p.Contains("register", StringComparison.Ordinal))
            return OperationLogType.Create;
        return OperationLogType.Other;
    }

    private static string Trunc(string? s, int maxLen)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= maxLen ? s : s[..maxLen];
    }
}

