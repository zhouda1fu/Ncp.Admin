using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 发布流程定义命令
/// </summary>
public record PublishWorkflowDefinitionCommand(WorkflowDefinitionId Id) : ICommand;

/// <summary>
/// 发布流程定义命令验证器
/// </summary>
public class PublishWorkflowDefinitionCommandValidator : AbstractValidator<PublishWorkflowDefinitionCommand>
{
    public PublishWorkflowDefinitionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程定义ID不能为空");
    }
}

/// <summary>
/// 发布流程定义命令处理器
/// </summary>
public class PublishWorkflowDefinitionCommandHandler(IWorkflowDefinitionRepository repository)
    : ICommandHandler<PublishWorkflowDefinitionCommand>
{
    public async Task Handle(PublishWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        definition.Publish();
    }
}
