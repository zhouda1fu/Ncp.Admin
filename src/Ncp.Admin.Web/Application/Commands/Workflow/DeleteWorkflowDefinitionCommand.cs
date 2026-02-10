using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 删除流程定义命令
/// </summary>
public record DeleteWorkflowDefinitionCommand(WorkflowDefinitionId Id) : ICommand;

/// <summary>
/// 删除流程定义命令验证器
/// </summary>
public class DeleteWorkflowDefinitionCommandValidator : AbstractValidator<DeleteWorkflowDefinitionCommand>
{
    public DeleteWorkflowDefinitionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程定义ID不能为空");
    }
}

/// <summary>
/// 删除流程定义命令处理器
/// </summary>
public class DeleteWorkflowDefinitionCommandHandler(
    IWorkflowDefinitionRepository repository,
    IMemoryCache memoryCache)
    : ICommandHandler<DeleteWorkflowDefinitionCommand>
{
    public async Task Handle(DeleteWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        definition.SoftDelete();

        memoryCache.Remove(WorkflowCacheKeys.DefinitionKey(request.Id));
        memoryCache.Remove(WorkflowCacheKeys.PublishedListKey);
    }
}
