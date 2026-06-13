using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using Serilog;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 根据 BusinessType 分发工作流业务回调，新增业务类型时只需要新增 IWorkflowBusinessAdapter。
/// </summary>
public class WorkflowBusinessAdapterDispatcher(IEnumerable<IWorkflowBusinessAdapter> handlers)
{
    private readonly Dictionary<string, IWorkflowBusinessAdapter> _handlers = handlers
        .GroupBy(h => h.BusinessType, StringComparer.Ordinal)
        .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

    /// <summary>
    /// 获取业务类型暴露给流程设计器的条件字段。
    /// </summary>
    public IReadOnlyList<ConditionFieldDto> GetConditionFields(string businessType)
    {
        return _handlers.TryGetValue(businessType, out var handler)
            ? handler.GetConditionFields()
            : [];
    }

    /// <summary>
    /// 获取全部已注册业务接入描述。
    /// </summary>
    public IReadOnlyList<WorkflowBusinessIntegrationDescriptor> GetIntegrations()
    {
        return _handlers.Values
            .Select(h => h.Integration)
            .OrderBy(i => i.BusinessType, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// 获取任务退回时的字段选择配置；未配置节点字段选择时返回 Disabled。
    /// </summary>
    /// <remarks>
    /// 退回字段由「审批节点配置」和「业务适配器」共同决定：节点只声明是否需要字段与方案编码，
    /// 实际字段白名单必须由业务适配器返回，通用工作流不理解订单等业务字段含义。
    /// </remarks>
    public async Task<WorkflowReturnOptionsDto> GetReturnOptionsAsync(
        WorkflowInstance instance,
        WorkflowTask task,
        Graph.WorkflowGraphNode? node,
        CancellationToken cancellationToken)
    {
        var mode = node?.ReturnFieldMode() ?? WorkflowReturnFieldModeNames.Disabled;
        var fieldSetCode = node?.ReturnFieldSetCode();
        if (!string.Equals(mode, WorkflowReturnFieldModeNames.Required, StringComparison.OrdinalIgnoreCase))
        {
            // Disabled 模式下不查询业务字段，退回时只保存说明，避免普通审批节点被业务字段方案影响。
            return new WorkflowReturnOptionsDto(WorkflowReturnFieldModeNames.Disabled, fieldSetCode, []);
        }

        var fields = _handlers.TryGetValue(instance.BusinessType, out var handler)
            ? await handler.GetReturnFieldOptionsAsync(instance, task, fieldSetCode, cancellationToken)
            : [];
        // Required 模式会把字段白名单返回给前端，退回提交时还会在 ReturnTaskCommand 中再次按 key 校验。
        return new WorkflowReturnOptionsDto(WorkflowReturnFieldModeNames.Required, fieldSetCode, fields);
    }

    /// <summary>
    /// 分发审批通过前业务动作。未注册业务类型时不做处理。
    /// </summary>
    public async Task DispatchBeforeTaskApprovedAsync(
        WorkflowTaskActionContext context,
        CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(context.Instance.BusinessType, out var handler))
        {
            await handler.OnBeforeTaskApprovedAsync(context, cancellationToken);
        }
    }

    /// <summary>
    /// 分发流程完成回调。未注册业务类型只记录 Debug 日志，不阻断通用工作流完成。
    /// </summary>
    public async Task DispatchCompletedAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(instance.BusinessType, out var handler))
        {
            await handler.OnCompletedAsync(instance, cancellationToken);
            return;
        }

        Log.Warning("未找到工作流完成业务适配器，流程实例ID：{InstanceId}，业务类型：{BusinessType}",
            instance.Id, instance.BusinessType);
    }

    /// <summary>
    /// 分发流程驳回回调。未注册业务类型只记录 Debug 日志。
    /// </summary>
    public async Task DispatchRejectedAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(instance.BusinessType, out var handler))
        {
            await handler.OnRejectedAsync(instance, cancellationToken);
            return;
        }

        Log.Warning("未找到工作流驳回业务适配器，流程实例ID：{InstanceId}，业务类型：{BusinessType}",
            instance.Id, instance.BusinessType);
    }

    /// <summary>
    /// 分发流程取消回调。业务无取消语义时处理器可保持默认空实现。
    /// </summary>
    public async Task DispatchCancelledAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(instance.BusinessType, out var handler))
        {
            await handler.OnCancelledAsync(instance, cancellationToken);
            return;
        }

        Log.Warning("未找到工作流取消业务适配器，流程实例ID：{InstanceId}，业务类型：{BusinessType}",
            instance.Id, instance.BusinessType);
    }
}
