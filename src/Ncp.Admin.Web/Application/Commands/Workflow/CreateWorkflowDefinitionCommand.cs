using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 创建流程定义命令
/// </summary>
public record CreateWorkflowDefinitionCommand(
    string Name,
    string Description,
    string Category,
    string DefinitionJson,
    UserId CreatedBy) : ICommand<WorkflowDefinitionId>;

/// <summary>
/// 创建流程定义命令验证器
/// </summary>
public class CreateWorkflowDefinitionCommandValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().WithMessage("流程名称不能为空")
            .MaximumLength(200).WithMessage("流程名称长度不能超过200个字符");
        RuleFor(c => c.Description).MaximumLength(500).WithMessage("流程描述长度不能超过500个字符");
        RuleFor(c => c.Category).MaximumLength(100).WithMessage("流程分类长度不能超过100个字符");
    }
}

/// <summary>
/// 创建流程定义命令处理器
/// </summary>
public class CreateWorkflowDefinitionCommandHandler(
    IWorkflowDefinitionRepository repository,
    WorkflowDefinitionAssigneeConfigValidator assigneeConfigValidator)
    : ICommandHandler<CreateWorkflowDefinitionCommand, WorkflowDefinitionId>
{
    public async Task<WorkflowDefinitionId> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        await assigneeConfigValidator.ValidateAsync(request.DefinitionJson, cancellationToken);

        var definition = new WorkflowDefinition(
            request.Name,
            request.Description,
            request.Category,
            request.DefinitionJson ?? string.Empty,
            request.CreatedBy);

        await repository.AddAsync(definition, cancellationToken);
        return definition.Id;
    }
}
