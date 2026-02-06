using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 驳回任务命令
/// </summary>
public record RejectTaskCommand(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId OperatorId,
    string Comment) : ICommand;

/// <summary>
/// 驳回任务命令验证器
/// </summary>
public class RejectTaskCommandValidator : AbstractValidator<RejectTaskCommand>
{
    public RejectTaskCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.TaskId).NotNull().WithMessage("任务ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
        RuleFor(c => c.Comment).NotEmpty().WithMessage("驳回时必须填写审批意见");
    }
}

/// <summary>
/// 驳回任务命令处理器
/// </summary>
public class RejectTaskCommandHandler(IWorkflowInstanceRepository instanceRepository)
    : ICommandHandler<RejectTaskCommand>
{
    public async Task Handle(RejectTaskCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.RejectTask(request.TaskId, request.OperatorId, request.Comment);
    }
}
