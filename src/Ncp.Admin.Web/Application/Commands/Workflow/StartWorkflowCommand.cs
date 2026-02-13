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
        if (string.IsNullOrEmpty(value)) return true;
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
/// Handler 负责编排，领域逻辑（获取首节点）下沉到聚合根，审批人由 WorkflowAssigneeResolverQuery 解析
/// </summary>
public class StartWorkflowCommandHandler(
    IWorkflowDefinitionRepository definitionRepository,
    IWorkflowInstanceRepository instanceRepository,
    WorkflowInstanceQuery instanceQuery,
    UserQuery userQuery,
    WorkflowAssigneeResolverQuery assigneeResolverQuery)
    : ICommandHandler<StartWorkflowCommand, WorkflowInstanceId>
{
    public async Task<WorkflowInstanceId> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        // 同一业务键防重：若已有运行中的流程则不允许重复发起
        var existsRunning = await instanceQuery.ExistsRunningInstanceByBusinessKeyAsync(
            request.BusinessType,
            request.BusinessKey,
            cancellationToken);
        if (existsRunning)
        {
            throw new KnownException("同一业务已有审批中的流程，请勿重复发起", ErrorCodes.WorkflowDuplicateBusinessKey);
        }

        // 获取流程定义
        var definition = await definitionRepository.GetAsync(request.WorkflowDefinitionId, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        if (definition.Status != WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("流程定义未发布，无法发起流程", ErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        // 获取发起人信息以获取部门ID
        var initiator = await userQuery.GetUserByIdAsync(request.InitiatorId, cancellationToken)
            ?? throw new KnownException("未找到发起人", ErrorCodes.UserNotFound);

        // 创建流程实例
        var instance = new WorkflowInstance(
            request.WorkflowDefinitionId,
            definition.Name,
            request.BusinessKey,
            request.BusinessType,
            request.Title,
            request.InitiatorId,
            request.InitiatorName,
            initiator.DeptId,
            request.Variables,
            request.Remark);

        await instanceRepository.AddAsync(instance, cancellationToken);

        // 通过聚合根领域方法解析条件分支，得到第一个需审批的节点
        var evaluator = WorkflowConditionEvaluator.CreateEvaluator(request.Variables);
        var firstNode = definition.GetFirstReachableApprovalNode(evaluator);
        if (firstNode != null)
        {
            if (firstNode.ApprovalMode == ApprovalMode.CounterSign)
            {
                // 会签：按人创建多条任务
                var assignees = await assigneeResolverQuery.ResolveAssigneesAsync(firstNode, instance, cancellationToken);
                foreach (var a in assignees)
                {
                    if (a.AssigneeId != null)
                        instance.CreateTask(firstNode.NodeName, WorkflowTaskType.Approval, a.AssigneeId, a.DisplayName);
                }
            }
            else
            {
                // 或签/依次：单条任务（指定用户或按角色查待办）
                var assignee = await assigneeResolverQuery.ResolveAssigneeAsync(firstNode, instance, cancellationToken);
                if (assignee != null)
                {
                    if (assignee.AssigneeId != null)
                        instance.CreateTask(firstNode.NodeName, WorkflowTaskType.Approval, assignee.AssigneeId!, assignee.DisplayName);
                    else
                        instance.CreateTaskForRole(firstNode.NodeName, WorkflowTaskType.Approval, assignee.AssigneeRoleId!, assignee.DisplayName);
                }
            }
        }

        return instance.Id;
    }
}
