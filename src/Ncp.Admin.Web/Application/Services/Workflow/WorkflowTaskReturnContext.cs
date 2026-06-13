using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 可退回字段选项，业务适配器按自身表单结构返回。
/// </summary>
/// <param name="Key">业务字段唯一 key，后端保存校验和前端控件放开都以该值匹配。</param>
/// <param name="Label">展示给审批人的字段名称，用于退回弹窗、详情和错误提示。</param>
/// <param name="Group">可选字段分组，仅用于前端分组展示，不参与通用工作流语义判断。</param>
public sealed record WorkflowReturnFieldDto(
    string Key,
    string Label,
    string? Group = null);

/// <summary>
/// 退回字段选择模式。
/// </summary>
public static class WorkflowReturnFieldModeNames
{
    /// <summary>
    /// 当前节点退回时不选择业务字段，只要求填写退回说明。
    /// </summary>
    public const string Disabled = "Disabled";

    /// <summary>
    /// 当前节点退回时必须从业务适配器返回的字段白名单中至少选择一项。
    /// </summary>
    public const string Required = "Required";
}

/// <summary>
/// 任务退回时的字段选择配置。
/// </summary>
/// <param name="FieldMode">当前审批节点的字段选择模式，决定退回命令是否强制校验 ReturnFields。</param>
/// <param name="FieldSetCode">业务字段方案编码，例如订单使用 orderApprovalReturnFields；通用工作流只透传不解释。</param>
/// <param name="Fields">当前业务在该字段方案下允许勾选的字段白名单。</param>
public sealed record WorkflowReturnOptionsDto(
    string FieldMode,
    string? FieldSetCode,
    IReadOnlyList<WorkflowReturnFieldDto> Fields);

/// <summary>
/// 保存在被退回节点新待办上的退回上下文。
/// </summary>
/// <param name="FieldMode">退回发生时的字段选择模式，用于后续业务编辑判断。</param>
/// <param name="FieldSetCode">退回发生时使用的业务字段方案编码。</param>
/// <param name="ReturnFields">本次退回审批人实际勾选的字段列表。</param>
/// <param name="Comment">审批人填写的退回说明。</param>
/// <param name="ReturnFromNodeKey">执行退回的审批节点 key。</param>
/// <param name="ReturnFromNodeName">执行退回的审批节点名称。</param>
/// <param name="ReturnToNodeKey">退回目标节点 key，可能是上一审批节点，也可能是开始节点。</param>
/// <param name="ReturnToNodeName">退回目标节点名称。</param>
/// <param name="ReturnedAt">退回发生时间，统一使用 UTC。</param>
public sealed record WorkflowTaskReturnContextDto(
    string FieldMode,
    string? FieldSetCode,
    IReadOnlyList<WorkflowReturnFieldDto> ReturnFields,
    string Comment,
    string ReturnFromNodeKey,
    string ReturnFromNodeName,
    string ReturnToNodeKey,
    string ReturnToNodeName,
    DateTimeOffset ReturnedAt);

/// <summary>
/// 任务扩展数据序列化工具，集中约定退回上下文在 ExtraDataJson 中的字段名。
/// </summary>
public static class WorkflowTaskExtraData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private sealed record WorkflowTaskExtraDataEnvelope(
        WorkflowTaskReturnContextDto? ReturnContext = null);

    /// <summary>
    /// 生成仅包含退回上下文的任务扩展数据。
    /// </summary>
    public static string CreateReturnContextJson(WorkflowTaskReturnContextDto returnContext)
    {
        return JsonSerializer.Serialize(new WorkflowTaskExtraDataEnvelope(returnContext), JsonOptions);
    }

    /// <summary>
    /// 从任务扩展数据中读取退回上下文；旧任务或无退回上下文时返回 null。
    /// </summary>
    public static WorkflowTaskReturnContextDto? TryReadReturnContext(string? extraDataJson)
    {
        if (string.IsNullOrWhiteSpace(extraDataJson))
        {
            return null;
        }

        try
        {
            // ExtraDataJson 未来可能承载更多扩展字段，退回上下文只读取 envelope 中的 returnContext。
            var envelope = JsonSerializer.Deserialize<WorkflowTaskExtraDataEnvelope>(extraDataJson, JsonOptions);
            return envelope?.ReturnContext;
        }
        catch (JsonException)
        {
            // 旧数据若不是当前 envelope 结构，按无退回上下文处理，避免影响正常审批。
            return null;
        }
    }
}
