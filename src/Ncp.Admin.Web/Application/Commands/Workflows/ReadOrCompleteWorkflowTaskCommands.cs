using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 标记抄送任务已读命令。
/// </summary>
public record ReadWorkflowTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    string Comment) : ICommand;

/// <summary>
/// 完成通知任务命令。
/// </summary>
public record CompleteWorkflowNotificationTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    string Comment) : ICommand;

/// <summary>
/// 标记抄送任务已读命令处理器。
/// </summary>
public class ReadWorkflowTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    WorkflowTaskOperationAuthorizer taskOperationAuthorizer)
    : ICommandHandler<ReadWorkflowTaskCommand>
{
    public async Task Handle(ReadWorkflowTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetWithTasksIgnoringQueryFiltersAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);
        var operatorRoleIds = await taskOperationAuthorizer.EnsureCanOperateAsync(
            instance,
            request.TaskId,
            request.OperatorId,
            cancellationToken);
        instance.ReadCarbonCopyTask(request.TaskId, request.OperatorId, operatorRoleIds, request.Comment);
    }
}

/// <summary>
/// 完成通知任务命令处理器。
/// </summary>
public class CompleteWorkflowNotificationTaskCommandHandler(
    IWorkflowInstanceRepository instanceRepository,
    WorkflowTaskOperationAuthorizer taskOperationAuthorizer)
    : ICommandHandler<CompleteWorkflowNotificationTaskCommand>
{
    public async Task Handle(CompleteWorkflowNotificationTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetWithTasksIgnoringQueryFiltersAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);
        var operatorRoleIds = await taskOperationAuthorizer.EnsureCanOperateAsync(
            instance,
            request.TaskId,
            request.OperatorId,
            cancellationToken);
        instance.CompleteNotificationTask(request.TaskId, request.OperatorId, operatorRoleIds, request.Comment);
    }
}
