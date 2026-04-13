using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 基于已存在的流程定义创建新版本命令
/// </summary>
/// <param name="SourceId">源流程定义ID</param>
public record CreateWorkflowDefinitionNewVersionCommand(WorkflowDefinitionId SourceId)
    : ICommand<WorkflowDefinitionId>;

/// <summary>
/// 基于已存在的流程定义创建新版本命令验证器
/// </summary>
public class CreateWorkflowDefinitionNewVersionCommandValidator
    : AbstractValidator<CreateWorkflowDefinitionNewVersionCommand>
{
    public CreateWorkflowDefinitionNewVersionCommandValidator()
    {
        RuleFor(c => c.SourceId)
            .NotNull()
            .WithMessage("源流程定义ID不能为空");
    }
}

/// <summary>
/// 基于已存在的流程定义创建新版本命令处理器
/// </summary>
public class CreateWorkflowDefinitionNewVersionCommandHandler(
    IWorkflowDefinitionRepository repository)
    : ICommandHandler<CreateWorkflowDefinitionNewVersionCommand, WorkflowDefinitionId>
{
    public async Task<WorkflowDefinitionId> Handle(
        CreateWorkflowDefinitionNewVersionCommand request,
        CancellationToken cancellationToken)
    {
        var source = await repository.GetAsync(request.SourceId, cancellationToken)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        var newDefinition = source.CreateNewVersion();

        await repository.AddAsync(newDefinition, cancellationToken);

        return newDefinition.Id;
    }
}

