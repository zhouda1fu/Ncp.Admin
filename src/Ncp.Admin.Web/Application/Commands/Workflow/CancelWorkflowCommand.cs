using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 撤销流程命令
/// </summary>
public record CancelWorkflowCommand(
    WorkflowInstanceId WorkflowInstanceId,
    UserId OperatorId) : ICommand;

/// <summary>
/// 撤销流程命令验证器
/// </summary>
public class CancelWorkflowCommandValidator : AbstractValidator<CancelWorkflowCommand>
{
    public CancelWorkflowCommandValidator()
    {
        RuleFor(c => c.WorkflowInstanceId).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
    }
}

/// <summary>
/// 撤销流程命令处理器
/// </summary>
public class CancelWorkflowCommandHandler(IWorkflowInstanceRepository instanceRepository)
    : ICommandHandler<CancelWorkflowCommand>
{
    public async Task Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.WorkflowInstanceId, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.Cancel(request.OperatorId);
    }
}
