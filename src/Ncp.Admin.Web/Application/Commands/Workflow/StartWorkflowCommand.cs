using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 发起流程命令
/// </summary>
public record StartWorkflowCommand(
    WorkflowDefinitionId WorkflowDefinitionId,
    string BusinessKey,
    string BusinessType,
    string Title,
    UserId InitiatorId,
    string InitiatorName,
    string Variables,
    string Remark) : ICommand<WorkflowInstanceId>;

/// <summary>
/// 发起流程命令验证器
/// </summary>
public class StartWorkflowCommandValidator : AbstractValidator<StartWorkflowCommand>
{
    private const int VariablesMaxLength = 64 * 1024; // 64KB

    public StartWorkflowCommandValidator()
    {
        RuleFor(c => c.WorkflowDefinitionId).NotNull().WithMessage("流程定义ID不能为空");
        RuleFor(c => c.Title).NotEmpty().WithMessage("流程标题不能为空")
            .MaximumLength(500).WithMessage("流程标题长度不能超过500个字符");
        RuleFor(c => c.InitiatorId).NotNull().WithMessage("发起人ID不能为空");
        RuleFor(c => c.Variables)
            .MaximumLength(VariablesMaxLength).WithMessage($"流程变量长度不能超过{VariablesMaxLength / 1024}KB");
        When(c => !string.IsNullOrEmpty(c.Variables), () =>
        {
            RuleFor(c => c.Variables).Must(BeValidJson).WithMessage("流程变量必须是有效的JSON格式");
        });
    }

    private static bool BeValidJson(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        try
        {
            System.Text.Json.JsonDocument.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// 发起流程命令处理器
/// 使用 WorkflowTreeTraverser 从设计器树 JSON 解析首个审批节点，审批人由 WorkflowAssigneeResolverQuery 解析。
/// </summary>
public class StartWorkflowCommandHandler(
    IWorkflowDefinitionRepository definitionRepository,
    IWorkflowInstanceRepository instanceRepository,
    WorkflowInstanceQuery instanceQuery,
    UserQuery userQuery,
    WorkflowTreeTraverser treeTraverser,
    WorkflowAssigneeResolverQuery assigneeResolverQuery)
    : ICommandHandler<StartWorkflowCommand, WorkflowInstanceId>
{
    public async Task<WorkflowInstanceId> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var existsRunning = await instanceQuery.ExistsRunningInstanceByBusinessKeyAsync(
            request.BusinessType,
            request.BusinessKey,
            cancellationToken);
        if (existsRunning)
        {
            throw new KnownException("同一业务已有审批中的流程，请勿重复发起", ErrorCodes.WorkflowDuplicateBusinessKey);
        }

        var definition = await definitionRepository.GetAsync(request.WorkflowDefinitionId, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        if (definition.Status != WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("流程定义未发布，无法发起流程", ErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        var initiator = await userQuery.GetUserByIdAsync(request.InitiatorId, cancellationToken)
            ?? throw new KnownException("未找到发起人", ErrorCodes.UserNotFound);

        var initiatorDisplayName = !string.IsNullOrWhiteSpace(initiator.RealName) ? initiator.RealName : initiator.Name;

        var instance = new WorkflowInstance(
            request.WorkflowDefinitionId,
            definition.Name,
            request.BusinessKey,
            request.BusinessType,
            request.Title,
            request.InitiatorId,
            initiatorDisplayName,
            initiator.DeptId,
            request.Variables,
            request.Remark);

        var node = treeTraverser.FindFirstTaskNode(definition.DefinitionJson, request.Variables);
        while (node != null)
        {
            var ordered = await assigneeResolverQuery.ResolveOrderedAssigneesAsync(node, instance, cancellationToken);
            if (node.Type == 1 && ordered.Count == 0)
            {
                throw new KnownException("无法解析审批节点的处理人，请检查流程配置", ErrorCodes.WorkflowAssigneeResolutionFailed);
            }

            var toCreate = WorkflowDesignerTaskHelper.SelectAssigneesForNodeEntry(node, ordered);
            var taskType = node.Type == 2 ? WorkflowTaskType.CarbonCopy : WorkflowTaskType.Approval;
            WorkflowDesignerTaskHelper.AddTasksToInstance(instance, node, taskType, toCreate);

            if (node.Type == 1)
            {
                break;
            }

            node = treeTraverser.FindNextTaskNode(definition.DefinitionJson, node.NodeKey, request.Variables);
        }

        if (instance.Tasks.Count == 0)
        {
            throw new KnownException("流程未生成任何待办任务，请检查流程定义是否包含审批或抄送节点", ErrorCodes.WorkflowNoTasksOnStart);
        }

        await instanceRepository.AddAsync(instance, cancellationToken);

        return instance.Id;
    }
}
