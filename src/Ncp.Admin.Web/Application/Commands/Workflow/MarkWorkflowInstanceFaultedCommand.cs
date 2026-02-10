using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 将流程实例标记为业务执行失败（Faulted）
/// </summary>
public record MarkWorkflowInstanceFaultedCommand(WorkflowInstanceId Id, string FailureReason) : ICommand;

/// <summary>
/// 命令验证器
/// </summary>
public class MarkWorkflowInstanceFaultedCommandValidator : AbstractValidator<MarkWorkflowInstanceFaultedCommand>
{
    public MarkWorkflowInstanceFaultedCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.FailureReason).NotEmpty().WithMessage("失败原因不能为空")
            .MaximumLength(2000).WithMessage("失败原因长度不能超过2000个字符");
    }
}

/// <summary>
/// 命令处理器
/// </summary>
public class MarkWorkflowInstanceFaultedCommandHandler(IWorkflowInstanceRepository instanceRepository)
    : ICommandHandler<MarkWorkflowInstanceFaultedCommand>
{
    public async Task Handle(MarkWorkflowInstanceFaultedCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.MarkFaulted(request.FailureReason);
    }
}
