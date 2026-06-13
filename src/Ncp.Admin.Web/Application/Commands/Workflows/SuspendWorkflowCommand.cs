using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 挂起流程命令
/// </summary>
public record SuspendWorkflowCommand(WorkflowInstanceId Id, UserId OperatorId) : ICommand;

/// <summary>
/// 挂起流程命令验证器
/// </summary>
public class SuspendWorkflowCommandValidator : AbstractValidator<SuspendWorkflowCommand>
{
    public SuspendWorkflowCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程实例ID不能为空");
        RuleFor(c => c.OperatorId).NotNull().WithMessage("操作人ID不能为空");
    }
}

/// <summary>
/// 挂起流程命令处理器
/// </summary>
public class SuspendWorkflowCommandHandler(IWorkflowInstanceRepository instanceRepository)
    : ICommandHandler<SuspendWorkflowCommand>
{
    public async Task Handle(SuspendWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetWithTasksIgnoringQueryFiltersAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.Suspend();
    }
}
