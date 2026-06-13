using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 获取任务退回字段选项查询。
/// </summary>
/// <param name="WorkflowInstanceId">待退回任务所属流程实例 ID，用于定位发布版本和业务类型。</param>
/// <param name="TaskId">当前审批待办 ID，必须是当前用户可操作的审批任务。</param>
/// <param name="OperatorId">当前操作人 ID，用于权限校验，避免无关用户读取业务字段白名单。</param>
public record GetWorkflowTaskReturnFieldsQuery(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId) : IQuery<WorkflowReturnOptionsDto>;

/// <summary>
/// 获取任务退回字段选项查询处理器。
/// </summary>
public class GetWorkflowTaskReturnFieldsQueryHandler(
    IWorkflowInstanceRepository workflowInstanceRepository,
    IWorkflowDefinitionRepository workflowDefinitionRepository,
    WorkflowGraphRuntimeService graphRuntimeService,
    WorkflowTaskOperationAuthorizer taskOperationAuthorizer,
    WorkflowBusinessAdapterDispatcher businessAdapterDispatcher)
    : IQueryHandler<GetWorkflowTaskReturnFieldsQuery, WorkflowReturnOptionsDto>
{
    public async Task<WorkflowReturnOptionsDto> Handle(
        GetWorkflowTaskReturnFieldsQuery request,
        CancellationToken cancellationToken)
    {
        var instance = await workflowInstanceRepository.GetWithTasksIgnoringQueryFiltersAsync(
                request.WorkflowInstanceId,
                cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        var task = instance.Tasks.FirstOrDefault(t => t.Id == request.TaskId)
            ?? throw new KnownException("未找到该任务", ErrorCodes.WorkflowTaskNotFound);
        if (task.TaskType != WorkflowTaskType.Approval)
        {
            throw new KnownException("只有审批任务可以退回", ErrorCodes.WorkflowTaskNotFound);
        }

        // 只有当前待办处理人才能查看退回字段，避免把业务字段范围暴露给无关用户。
        await taskOperationAuthorizer.EnsureCanOperateAsync(
            instance,
            request.TaskId,
            request.OperatorId,
            cancellationToken);

        var definitionVersion = await workflowDefinitionRepository.GetVersionAsync(
                instance.WorkflowDefinitionVersionId,
                cancellationToken)
            ?? throw new KnownException("未找到流程定义版本，无法获取退回配置", ErrorCodes.WorkflowDefinitionNotFound);

        // 退回字段配置挂在当前审批节点运行图扩展上，查询端和提交端都使用同一套 dispatcher 解析，避免前后规则分叉。
        var currentNode = graphRuntimeService.FindNodeByKey(definitionVersion.GraphSnapshotJson, task.NodeKey);
        return await businessAdapterDispatcher.GetReturnOptionsAsync(instance, task, currentNode, cancellationToken);
    }
}
