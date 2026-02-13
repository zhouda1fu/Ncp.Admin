using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Workflow;

/// <summary>
/// 流程节点请求数据
/// </summary>
public record WorkflowNodeData(
    string NodeName,
    WorkflowNodeType NodeType,
    AssigneeType AssigneeType,
    string AssigneeValue,
    int SortOrder,
    string Description,
    ApprovalMode ApprovalMode = ApprovalMode.OrSign,
    string? ConditionExpression = null,
    string? TrueNextNodeName = null,
    string? FalseNextNodeName = null);

/// <summary>
/// 创建流程定义命令
/// </summary>
public record CreateWorkflowDefinitionCommand(
    string Name,
    string Description,
    string Category,
    string DefinitionJson,
    UserId CreatedBy,
    IEnumerable<WorkflowNodeData> Nodes) : ICommand<WorkflowDefinitionId>;

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
/// 与 CreateRoleCommandHandler 模式一致：在 Handler 中构建子集合，通过构造函数传入聚合根
/// </summary>
public class CreateWorkflowDefinitionCommandHandler(IWorkflowDefinitionRepository repository)
    : ICommandHandler<CreateWorkflowDefinitionCommand, WorkflowDefinitionId>
{
    public async Task<WorkflowDefinitionId> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        // 构建节点集合（与 Role + RolePermission 模式一致）
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

        var definition = new WorkflowDefinition(
            request.Name,
            request.Description,
            request.Category,
            request.DefinitionJson,
            request.CreatedBy,
            nodes);

        await repository.AddAsync(definition, cancellationToken);
        return definition.Id;
    }
}
