using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

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
/// Handler 负责编排，流程流转逻辑（获取下一节点）下沉到 WorkflowDefinition 聚合根，审批人由 WorkflowAssigneeResolverQuery 解析
/// </summary>
public class ApproveTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    IWorkflowDefinitionRepository definitionRepository,
    WorkflowAssigneeResolverQuery assigneeResolverQuery)
    : ICommandHandler<ApproveTaskCommand>
{
    public async Task Handle(ApproveTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.ApproveTask(request.TaskId, request.OperatorId, request.Comment);

        var definition = await definitionRepository.GetAsync(instance.WorkflowDefinitionId, cancellationToken);
        if (definition == null) return;

        var approvedTask = instance.Tasks.First(t => t.Id == request.TaskId);
        var currentNode = definition.GetOrderedApprovalNodes().FirstOrDefault(n => n.NodeName == approvedTask.NodeName);

        // 会签：仅当当前节点所有任务均已通过时才进入下一节点或完成
        if (currentNode?.ApprovalMode == ApprovalMode.CounterSign && !instance.AreAllCounterSignTasksApproved(approvedTask.NodeName))
            return;

        var nextNode = definition.GetNextApprovalNode(approvedTask.NodeName);

        if (nextNode != null)
        {
            if (nextNode.ApprovalMode == ApprovalMode.CounterSign)
            {
                var assignees = await assigneeResolverQuery.ResolveAssigneesAsync(nextNode, instance, cancellationToken);
                foreach (var a in assignees)
                {
                    if (a.AssigneeId != null)
                        instance.CreateTask(nextNode.NodeName, WorkflowTaskType.Approval, a.AssigneeId, a.DisplayName);
                }
            }
            else
            {
                var assignee = await assigneeResolverQuery.ResolveAssigneeAsync(nextNode, instance, cancellationToken);
                if (assignee != null)
                {
                    if (assignee.AssigneeId != null)
                        instance.CreateTask(nextNode.NodeName, WorkflowTaskType.Approval, assignee.AssigneeId!, assignee.DisplayName);
                    else
                        instance.CreateTaskForRole(nextNode.NodeName, WorkflowTaskType.Approval, assignee.AssigneeRoleId!, assignee.DisplayName);
                }
            }
        }
        else
        {
            instance.Complete();
        }
    }
}
