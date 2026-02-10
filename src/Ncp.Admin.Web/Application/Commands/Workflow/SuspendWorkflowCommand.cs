using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 挂起流程命令
/// </summary>
public record SuspendWorkflowCommand(WorkflowInstanceId Id) : ICommand;

/// <summary>
/// 挂起流程命令验证器
/// </summary>
public class SuspendWorkflowCommandValidator : AbstractValidator<SuspendWorkflowCommand>
{
    public SuspendWorkflowCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程实例ID不能为空");
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
        var instance = await instanceRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.Suspend();
    }
}
