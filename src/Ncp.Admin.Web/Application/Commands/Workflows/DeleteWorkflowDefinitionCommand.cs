using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 删除草稿流程定义命令
/// </summary>
/// <param name="Id">流程定义 ID</param>
public record DeleteDraftWorkflowDefinitionCommand(WorkflowDefinitionId Id) : ICommand;

/// <summary>
/// 删除草稿流程定义命令验证器
/// </summary>
public class DeleteDraftWorkflowDefinitionCommandValidator : AbstractValidator<DeleteDraftWorkflowDefinitionCommand>
{
    public DeleteDraftWorkflowDefinitionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程定义ID不能为空");
    }
}

/// <summary>
/// 删除草稿流程定义命令处理器
/// </summary>
public class DeleteDraftWorkflowDefinitionCommandHandler(
    IWorkflowDefinitionRepository repository,
    WorkflowDefinitionCacheInvalidator cacheInvalidator)
    : ICommandHandler<DeleteDraftWorkflowDefinitionCommand>
{
    public async Task Handle(DeleteDraftWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        definition.SoftDeleteDraft();

        cacheInvalidator.InvalidateDefinitionWrite(request.Id);
    }
}

/// <summary>
/// 删除已发布或已归档流程定义命令
/// </summary>
/// <param name="Id">流程定义 ID</param>
public record DeletePublishedWorkflowDefinitionCommand(WorkflowDefinitionId Id) : ICommand;

/// <summary>
/// 删除已发布或已归档流程定义命令验证器
/// </summary>
public class DeletePublishedWorkflowDefinitionCommandValidator
    : AbstractValidator<DeletePublishedWorkflowDefinitionCommand>
{
    public DeletePublishedWorkflowDefinitionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程定义ID不能为空");
    }
}

/// <summary>
/// 删除已发布或已归档流程定义命令处理器
/// </summary>
public class DeletePublishedWorkflowDefinitionCommandHandler(
    IWorkflowDefinitionRepository repository,
    WorkflowDefinitionCacheInvalidator cacheInvalidator)
    : ICommandHandler<DeletePublishedWorkflowDefinitionCommand>
{
    public async Task Handle(DeletePublishedWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        definition.SoftDeletePublishedOrArchived();

        cacheInvalidator.InvalidateDefinitionWrite(request.Id);
    }
}
