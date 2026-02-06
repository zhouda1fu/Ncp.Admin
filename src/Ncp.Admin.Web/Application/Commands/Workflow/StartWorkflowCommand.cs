using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

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
    public StartWorkflowCommandValidator()
    {
        RuleFor(c => c.WorkflowDefinitionId).NotNull().WithMessage("流程定义ID不能为空");
        RuleFor(c => c.Title).NotEmpty().WithMessage("流程标题不能为空")
            .MaximumLength(500).WithMessage("流程标题长度不能超过500个字符");
        RuleFor(c => c.InitiatorId).NotNull().WithMessage("发起人ID不能为空");
    }
}

/// <summary>
/// 发起流程命令处理器
/// Handler 负责编排，领域逻辑（获取首节点）下沉到聚合根
/// </summary>
public class StartWorkflowCommandHandler(
    IWorkflowDefinitionRepository definitionRepository,
    IWorkflowInstanceRepository instanceRepository)
    : ICommandHandler<StartWorkflowCommand, WorkflowInstanceId>
{
    public async Task<WorkflowInstanceId> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        // 获取流程定义
        var definition = await definitionRepository.GetAsync(request.WorkflowDefinitionId, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        if (definition.Status != WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("流程定义未发布，无法发起流程", ErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        // 创建流程实例
        var instance = new WorkflowInstance(
            request.WorkflowDefinitionId,
            definition.Name,
            request.BusinessKey,
            request.BusinessType,
            request.Title,
            request.InitiatorId,
            request.InitiatorName,
            request.Variables,
            request.Remark);

        await instanceRepository.AddAsync(instance, cancellationToken);

        // 通过聚合根领域方法获取第一个审批节点
        var firstNode = definition.GetFirstApprovalNode();

        if (firstNode != null
            && long.TryParse(firstNode.AssigneeValue, out var assigneeIdValue))
        {
            var assigneeId = new UserId(assigneeIdValue);
            instance.CreateTask(
                firstNode.NodeName,
                WorkflowTaskType.Approval,
                assigneeId,
                string.Empty);
        }

        return instance.Id;
    }
}
