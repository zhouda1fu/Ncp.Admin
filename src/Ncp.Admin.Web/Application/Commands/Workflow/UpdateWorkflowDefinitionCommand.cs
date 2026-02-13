using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 更新流程定义命令
/// </summary>
public record UpdateWorkflowDefinitionCommand(
    WorkflowDefinitionId Id,
    string Name,
    string Description,
    string Category,
    string DefinitionJson,
    IEnumerable<WorkflowNodeData> Nodes) : ICommand;

/// <summary>
/// 更新流程定义命令验证器
/// </summary>
public class UpdateWorkflowDefinitionCommandValidator : AbstractValidator<UpdateWorkflowDefinitionCommand>
{
    public UpdateWorkflowDefinitionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("流程定义ID不能为空");
        RuleFor(c => c.Name).NotEmpty().WithMessage("流程名称不能为空")
            .MaximumLength(200).WithMessage("流程名称长度不能超过200个字符");
        RuleFor(c => c.Description).MaximumLength(500).WithMessage("流程描述长度不能超过500个字符");
        RuleFor(c => c.Category).MaximumLength(100).WithMessage("流程分类长度不能超过100个字符");
    }
}

/// <summary>
/// 更新流程定义命令处理器
/// </summary>
public class UpdateWorkflowDefinitionCommandHandler(
    IWorkflowDefinitionRepository repository,
    IMemoryCache memoryCache)
    : ICommandHandler<UpdateWorkflowDefinitionCommand>
{
    public async Task Handle(UpdateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        var nodes = request.Nodes.Select(n => new WorkflowNode(
            n.NodeName,
            n.NodeType,
            n.AssigneeType,
            n.AssigneeValue,
            n.SortOrder,
            n.Description,
            n.ApprovalMode,
            n.ConditionExpression,
            n.TrueNextNodeName,
            n.FalseNextNodeName));

        definition.UpdateInfo(request.Name, request.Description, request.Category, request.DefinitionJson, nodes);

        memoryCache.Remove(WorkflowCacheKeys.DefinitionKey(request.Id));
    }
}
