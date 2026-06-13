using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 工作流业务接入点。每个 BusinessType 只注册一个处理器，集中承载完成、驳回、取消和条件字段契约。
/// </summary>
public interface IWorkflowBusinessAdapter
{
    /// <summary>
    /// 处理器对应的工作流业务类型，必须与流程实例的 BusinessType 一致。
    /// </summary>
    string BusinessType { get; }

    /// <summary>
    /// 业务接入描述，用于集中查看业务类型、条件字段、回调能力和动作负载契约。
    /// </summary>
    WorkflowBusinessIntegrationDescriptor Integration =>
        WorkflowBusinessIntegrationDescriptor.Create(BusinessType, GetConditionFields());

    /// <summary>
    /// 返回该业务类型可用于条件路由的变量字段。
    /// </summary>
    IReadOnlyList<ConditionFieldDto> GetConditionFields() => [];

    /// <summary>
    /// 返回该业务类型在退回时可勾选的字段范围。
    /// </summary>
    /// <remarks>
    /// 通用工作流只负责保存字段 key 和 label，不理解具体业务含义；
    /// 每个业务适配器必须保证这里返回的 key 能被自身保存逻辑识别并校验。
    /// 未实现时默认返回空集合，表示该业务没有可选退回字段。
    /// </remarks>
    Task<IReadOnlyList<WorkflowReturnFieldDto>> GetReturnFieldOptionsAsync(
        WorkflowInstance instance,
        WorkflowTask task,
        string? fieldSetCode,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<WorkflowReturnFieldDto>>([]);

    /// <summary>
    /// 审批任务通过前调用。业务可读取动作扩展负载并完成前置校验或业务准备。
    /// </summary>
    Task OnBeforeTaskApprovedAsync(WorkflowTaskActionContext context, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <summary>
    /// 流程审批通过并完成后调用，用于回写业务状态或执行业务动作。
    /// </summary>
    Task OnCompletedAsync(WorkflowInstance instance, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// 流程被驳回后调用，用于回写业务驳回状态。
    /// </summary>
    Task OnRejectedAsync(WorkflowInstance instance, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// 流程被发起人取消后调用。多数业务无需处理时可保持默认空实现。
    /// </summary>
    Task OnCancelledAsync(WorkflowInstance instance, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// 工作流任务动作上下文。通用工作流只传递动作负载，具体业务由对应处理器解释。
/// </summary>
public sealed record WorkflowTaskActionContext(
    WorkflowInstance Instance,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    IReadOnlyCollection<RoleId> OperatorRoleIds,
    IReadOnlyDictionary<string, JsonElement> ActionPayload);

/// <summary>
/// 工作流业务接入描述。
/// </summary>
public sealed record WorkflowBusinessIntegrationDescriptor(
    string BusinessType,
    IReadOnlyList<ConditionFieldDto> ConditionFields,
    IReadOnlyList<string> CallbackNames,
    IReadOnlyList<WorkflowActionPayloadSchemaDto> ActionPayloadSchemas)
{
    /// <summary>
    /// 创建默认业务接入描述。
    /// </summary>
    public static WorkflowBusinessIntegrationDescriptor Create(
        string businessType,
        IReadOnlyList<ConditionFieldDto> conditionFields,
        IReadOnlyList<string>? callbackNames = null,
        IReadOnlyList<WorkflowActionPayloadSchemaDto>? actionPayloadSchemas = null)
    {
        return new WorkflowBusinessIntegrationDescriptor(
            businessType,
            conditionFields,
            callbackNames ?? [WorkflowBusinessCallbackNames.Completed, WorkflowBusinessCallbackNames.Rejected, WorkflowBusinessCallbackNames.Cancelled],
            actionPayloadSchemas ?? []);
    }
}

/// <summary>
/// 审批动作扩展负载字段描述。
/// </summary>
public sealed record WorkflowActionPayloadSchemaDto(
    string PayloadKey,
    string FieldPath,
    string FieldType,
    bool Required,
    string Description);

/// <summary>
/// 工作流业务回调名称。
/// </summary>
public static class WorkflowBusinessCallbackNames
{
    public const string BeforeTaskApproved = "BeforeTaskApproved";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
