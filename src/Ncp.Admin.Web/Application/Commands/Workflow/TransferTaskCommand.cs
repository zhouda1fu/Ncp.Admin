using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 转办任务命令
/// </summary>
public record TransferTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    UserId NewAssigneeId,
    string NewAssigneeName,
    string Comment) : ICommand;

/// <summary>
/// 转办任务命令验证器
/// </summary>
public class TransferTaskCommandValidator : AbstractValidator<TransferTaskCommand>
{
    public TransferTaskCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.TaskId).NotNull().WithMessage("任务ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
        RuleFor(c => c.NewAssigneeId).NotNull().WithMessage("新处理人ID不能为空");
        RuleFor(c => c.NewAssigneeName).NotEmpty().WithMessage("新处理人姓名不能为空");
    }
}

/// <summary>
/// 转办任务命令处理器
/// </summary>
public class TransferTaskCommandHandler(IWorkflowInstanceRepository instanceRepository)
    : ICommandHandler<TransferTaskCommand>
{
    public async Task Handle(TransferTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.TransferTask(request.TaskId, request.NewAssigneeId, request.NewAssigneeName, request.Comment);
    }
}
