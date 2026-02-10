using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 恢复流程命令
/// </summary>
public record ResumeWorkflowCommand(WorkflowInstanceId Id) : ICommand;

/// <summary>
/// 恢复流程命令验证器
/// </summary>
public class ResumeWorkflowCommandValidator : AbstractValidator<ResumeWorkflowCommand>
{
    public ResumeWorkflowCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程实例ID不能为空");
    }
}

/// <summary>
/// 恢复流程命令处理器
/// </summary>
public class ResumeWorkflowCommandHandler(IWorkflowInstanceRepository instanceRepository)
    : ICommandHandler<ResumeWorkflowCommand>
{
    public async Task Handle(ResumeWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程实例", ErrorCodes.WorkflowInstanceNotFound);

        instance.Resume();
    }
}
