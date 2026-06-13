using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 归档流程定义命令。
/// </summary>
public record ArchiveWorkflowDefinitionCommand(WorkflowDefinitionId Id) : ICommand;

/// <summary>
/// 归档流程定义命令验证器。
/// </summary>
public class ArchiveWorkflowDefinitionCommandValidator : AbstractValidator<ArchiveWorkflowDefinitionCommand>
{
    public ArchiveWorkflowDefinitionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程定义ID不能为空");
    }
}

/// <summary>
/// 归档流程定义命令处理器。
/// </summary>
public class ArchiveWorkflowDefinitionCommandHandler(
    IWorkflowDefinitionRepository repository,
    WorkflowDefinitionCacheInvalidator cacheInvalidator)
    : ICommandHandler<ArchiveWorkflowDefinitionCommand>
{
    public async Task Handle(ArchiveWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await repository.GetAsync(request.Id, cancellationToken);
        if (definition == null || definition.Status == WorkflowDefinitionStatus.Archived)
        {
            return;
        }

        definition.Archive();
        cacheInvalidator.InvalidateDefinitionWrite(request.Id);
    }
}
