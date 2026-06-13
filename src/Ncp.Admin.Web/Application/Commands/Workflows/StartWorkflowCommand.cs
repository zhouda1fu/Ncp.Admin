using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using Npgsql;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

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
/// 使用已发布的 GraphSnapshotJson 解析首个待办节点，避免运行时依赖可编辑的设计器 JSON。
/// </summary>
public class StartWorkflowCommandHandler(
    IWorkflowDefinitionRepository definitionRepository,
    IWorkflowInstanceRepository instanceRepository,
    WorkflowInstanceQuery instanceQuery,
    UserQuery userQuery,
    IWorkflowApprovalAssignmentService approvalAssignmentService,
    WorkflowRuntimeRecordService runtimeRecordService,
    WorkflowGraphRuntimeService graphRuntimeService)
    : ICommandHandler<StartWorkflowCommand, WorkflowInstanceId>
{
    private const string DuplicateBusinessWorkflowMessage = "同一业务已有审批中的流程，请勿重复发起";

    public async Task<WorkflowInstanceId> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var existsRunning = await instanceQuery.ExistsRunningInstanceByBusinessKeyAsync(
            request.BusinessType,
            request.BusinessKey,
            cancellationToken);
        if (existsRunning)
        {
            throw new KnownException(DuplicateBusinessWorkflowMessage, ErrorCodes.WorkflowDuplicateBusinessKey);
        }

        var definition = await definitionRepository.GetAsync(request.WorkflowDefinitionId, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        if (definition.Status != WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("流程定义未发布，无法发起流程", ErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        var definitionVersion = definition.GetLatestPublishedVersion()
            ?? throw new KnownException("流程定义缺少已发布版本，无法发起流程", ErrorCodes.WorkflowDefinitionNotFound);

        var initiator = await userQuery.GetUserByIdAsync(request.InitiatorId, cancellationToken)
            ?? throw new KnownException("未找到发起人", ErrorCodes.UserNotFound);

        var initiatorDisplayName = !string.IsNullOrWhiteSpace(initiator.RealName) ? initiator.RealName : initiator.Name;

        var instance = new WorkflowInstance(
            request.WorkflowDefinitionId,
            definitionVersion.Id,
            definition.Name,
            request.BusinessKey,
            request.BusinessType,
            request.Title,
            request.InitiatorId,
            initiatorDisplayName,
            initiator.DeptId,
            request.Variables,
            request.Remark);

        await instanceRepository.AddAsync(instance, cancellationToken);

        var graphSnapshotJson = definitionVersion.GraphSnapshotJson;
        if (string.IsNullOrWhiteSpace(graphSnapshotJson))
        {
            throw new KnownException("流程定义缺少已发布的运行图快照，无法发起流程", ErrorCodes.WorkflowDefinitionNotFound);
        }

        var node = graphRuntimeService.FindFirstTaskNode(graphSnapshotJson, request.Variables);
        while (node != null)
        {
            if (WorkflowStartAssigneeGate.IsOfficeTaskParticipantConfigNode(node))
            {
                node = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, node.NodeId, request.Variables);
                continue;
            }

            var resolution = await approvalAssignmentService.ResolveForTaskCreationAsync(
                node,
                instance,
                graphSnapshotJson,
                cancellationToken);
            var toCreate = WorkflowDesignerTaskHelper.SelectAssigneesForNodeEntry(node, resolution.Assignees);
            var taskType = node.Type == WorkflowGraphNodeType.CarbonCopy
                ? WorkflowTaskType.CarbonCopy
                : WorkflowTaskType.Approval;
            var createdTasks = WorkflowDesignerTaskHelper.AddTaskAssignmentsToInstance(instance, node, taskType, toCreate);
            await runtimeRecordService.RecordTaskCreatedAsync(
                instance,
                createdTasks,
                "start",
                cancellationToken);

            if (node.Type == WorkflowGraphNodeType.Approval && !resolution.AutoPassed)
            {
                break;
            }

            node = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, node.NodeId, request.Variables);
        }

        if (instance.Tasks.Count == 0)
        {
            if (!graphRuntimeService.AllowsAutoCompleteWithoutTasks(graphSnapshotJson))
            {
                throw new KnownException("流程未产生任何待办，请检查流程定义审批/抄送配置", ErrorCodes.WorkflowAssigneeResolutionFailed);
            }

            instance.Complete();
        }

        return instance.Id;
    }
}

/// <summary>
/// 将并发发起时数据库唯一索引兜底转换成业务异常。
/// </summary>
public class StartWorkflowDuplicateBusinessKeyBehavior : IPipelineBehavior<StartWorkflowCommand, WorkflowInstanceId>
{
    private const string DuplicateBusinessWorkflowMessage = "同一业务已有审批中的流程，请勿重复发起";
    private const string UniqueViolationSqlState = "23505";
    private const string ActiveBusinessIndexName = "ix_workflow_instance_active_business";

    public async Task<WorkflowInstanceId> Handle(
        StartWorkflowCommand request,
        RequestHandlerDelegate<WorkflowInstanceId> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DbUpdateException ex) when (IsActiveBusinessUniqueViolation(ex))
        {
            throw new KnownException(DuplicateBusinessWorkflowMessage, ErrorCodes.WorkflowDuplicateBusinessKey);
        }
    }

    private static bool IsActiveBusinessUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: UniqueViolationSqlState,
            ConstraintName: var constraintName
        } && string.Equals(constraintName, ActiveBusinessIndexName, StringComparison.OrdinalIgnoreCase);
    }
}
