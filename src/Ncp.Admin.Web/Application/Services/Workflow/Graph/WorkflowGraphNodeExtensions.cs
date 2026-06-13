using System.Text.Json;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Services.Workflow.Graph;

/// <summary>
/// 从 <see cref="WorkflowGraphNode.ExtensionsJson"/> 解析业务扩展字段。
/// </summary>
public static class WorkflowGraphNodeExtensions
{
    /// <summary>
    /// 当前节点退回时的字段选择模式：Disabled / Required。
    /// </summary>
    /// <remarks>
    /// 未配置 workflowReturn.fieldMode 时默认 Disabled，避免旧流程定义在发布快照升级后被误判为必须选择字段。
    /// </remarks>
    public static string ReturnFieldMode(this WorkflowGraphNode node) =>
        TryGetWorkflowReturnString(node, "fieldMode") is { Length: > 0 } mode
            ? mode
            : WorkflowReturnFieldModeNames.Disabled;

    /// <summary>
    /// 当前节点退回字段方案编码。
    /// </summary>
    /// <remarks>
    /// 该值由业务适配器解释；通用工作流只负责从运行图扩展中读取并透传。
    /// </remarks>
    public static string? ReturnFieldSetCode(this WorkflowGraphNode node) =>
        TryGetWorkflowReturnString(node, "fieldSetCode");

    /// <summary>平台精简版无办公任务扩展。</summary>
    public static bool OfficeTaskParticipantNode(this WorkflowGraphNode node) => false;

    /// <summary>平台精简版无办公任务扩展。</summary>
    public static string? OfficeTaskReceiverConfigMode(this WorkflowGraphNode node) => null;

    /// <summary>平台精简版无办公任务扩展。</summary>
    public static string? OfficeTaskCarbonCopyConfigMode(this WorkflowGraphNode node) => null;

    private static string? TryGetWorkflowReturnString(WorkflowGraphNode node, string property) =>
        TryGetWorkflowReturnProperty(node, property, out var value) ? value.GetString() : null;

    private static bool TryGetWorkflowReturnProperty(
        WorkflowGraphNode node,
        string property,
        out JsonElement value)
    {
        value = default;
        if (!TryGetRootObject(node.ExtensionsJson, out var root)
            || !root.TryGetProperty("workflowReturn", out var workflowReturn)
            || workflowReturn.ValueKind != JsonValueKind.Object
            || !workflowReturn.TryGetProperty(property, out value))
        {
            return false;
        }

        return true;
    }

    private static bool TryGetRootObject(string? extensionsJson, out JsonElement root)
    {
        root = default;
        if (string.IsNullOrWhiteSpace(extensionsJson))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(extensionsJson);
            root = doc.RootElement.Clone();
            return root.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
