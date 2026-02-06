using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 审批通过命令
/// </summary>
public record ApproveTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    string Comment) : ICommand;

/// <summary>
/// 审批通过命令验证器
/// </summary>
public class ApproveTaskCommandValidator : AbstractValidator<ApproveTaskCommand>
{
    public ApproveTaskCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.TaskId).NotNull().WithMessage("任务ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
    }
}

/// <summary>
/// 审批通过命令处理器
/// Handler 负责编排，流程流转逻辑（获取下一节点）下沉到 WorkflowDefinition 聚合根
/// </summary>
public class ApproveTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    IWorkflowDefinitionRepository definitionRepository)
    : ICommandHandler<ApproveTaskCommand>
{
    public async Task Handle(ApproveTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        // 通过聚合根方法审批任务
        instance.ApproveTask(request.TaskId, request.OperatorId, request.Comment);

        // 通过流程定义聚合根的领域方法获取下一节点
        var definition = await definitionRepository.GetAsync(instance.WorkflowDefinitionId, cancellationToken);
        if (definition == null) return;

        var approvedTask = instance.Tasks.First(t => t.Id == request.TaskId);
        var nextNode = definition.GetNextApprovalNode(approvedTask.NodeName);

        if (nextNode != null)
        {
            // 还有下一个审批节点，创建新任务
            if (long.TryParse(nextNode.AssigneeValue, out var assigneeIdValue))
            {
                var assigneeId = new UserId(assigneeIdValue);
                instance.CreateTask(
                    nextNode.NodeName,
                    WorkflowTaskType.Approval,
                    assigneeId,
                    string.Empty);
            }
        }
        else
        {
            // 所有审批节点都已通过，完成流程
            instance.Complete();
        }
    }
}
