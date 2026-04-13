using System.Text.Json;
using FluentValidation;
using MediatR;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 发起客户作废审批流程（审批通过后再执行实际作废）
/// </summary>
public record StartCustomerSeaVoidWorkflowCommand(
    CustomerId CustomerId,
    UserId OperatorUserId,
    RoleId? RoutingRoleId) : ICommand<StartCustomerSeaVoidWorkflowResult>;

/// <param name="WorkflowInstanceId">工作流实例 ID</param>
/// <param name="Title">流程标题</param>
public record StartCustomerSeaVoidWorkflowResult(WorkflowInstanceId WorkflowInstanceId, string Title);

public class StartCustomerSeaVoidWorkflowCommandValidator : AbstractValidator<StartCustomerSeaVoidWorkflowCommand>
{
    public StartCustomerSeaVoidWorkflowCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.OperatorUserId).NotEmpty();
    }
}

/// <summary>
/// 客户作废流程变量（存入工作流 Variables JSON，与设计器条件字段一致）
/// </summary>
public class CustomerSeaVoidWorkflowVariables
{
    public string CustomerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? MainContactName { get; set; }
    public string RoutingRoleId { get; set; } = string.Empty;
}

public class StartCustomerSeaVoidWorkflowCommandHandler(
    ICustomerRepository customerRepository,
    WorkflowDefinitionQuery workflowDefinitionQuery,
    UserQuery userQuery,
    IMediator mediator)
    : ICommandHandler<StartCustomerSeaVoidWorkflowCommand, StartCustomerSeaVoidWorkflowResult>
{
    public async Task<StartCustomerSeaVoidWorkflowResult> Handle(
        StartCustomerSeaVoidWorkflowCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);

        EnsureMaySubmitVoidWorkflow(customer, request.OperatorUserId);

        var definition = await workflowDefinitionQuery.GetFirstPublishedByCategoryAsync(
                WorkflowBusinessTypes.CustomerSeaVoid, cancellationToken)
            ?? throw new KnownException(
                "未配置客户作废审批流程，请在流程定义中发布分类为「客户作废」的流程",
                ErrorCodes.CustomerSeaVoidWorkflowNotConfigured);

        var operatorRoleIds = await userQuery.GetRoleIdsByUserIdAsync(request.OperatorUserId, cancellationToken);
        if (operatorRoleIds.Count == 0)
        {
            throw new KnownException("当前用户未分配角色，无法发起作废审批", ErrorCodes.CustomerSeaVoidOperatorHasNoRoles);
        }

        RoleId resolvedRoutingRole;
        if (operatorRoleIds.Count == 1)
        {
            resolvedRoutingRole = operatorRoleIds[0];
        }
        else
        {
            if (request.RoutingRoleId is null || request.RoutingRoleId == new RoleId(Guid.Empty))
            {
                throw new KnownException("请选择本次作废审批使用的角色", ErrorCodes.CustomerSeaVoidRoutingRoleRequired);
            }

            if (!operatorRoleIds.Contains(request.RoutingRoleId))
            {
                throw new KnownException("所选角色不属于当前用户", ErrorCodes.CustomerSeaVoidRoutingRoleNotAssignedToUser);
            }

            resolvedRoutingRole = request.RoutingRoleId;
        }

        var initiator = await userQuery.GetUserByIdAsync(request.OperatorUserId, cancellationToken);
        var initiatorDisplayName = !string.IsNullOrWhiteSpace(initiator.RealName) ? initiator.RealName : initiator.Name;

        var variables = new CustomerSeaVoidWorkflowVariables
        {
            CustomerId = customer.Id.ToString(),
            FullName = customer.FullName,
            MainContactName = customer.MainContactName,
            RoutingRoleId = resolvedRoutingRole.ToString(),
        };
        var variablesJson = JsonSerializer.Serialize(variables);

        var title = $"客户作废审批-{customer.FullName}";

        var instanceId = await mediator.Send(
            new StartWorkflowCommand(
                definition.Id,
                customer.Id.ToString(),
                WorkflowBusinessTypes.CustomerSeaVoid,
                title,
                request.OperatorUserId,
                initiatorDisplayName,
                variablesJson,
                string.Empty),
            cancellationToken);

        return new StartCustomerSeaVoidWorkflowResult(instanceId, title);
    }

    private static void EnsureMaySubmitVoidWorkflow(Customer customer, UserId operatorId)
    {
        if (customer.IsVoided)
        {
            throw new KnownException("客户已作废，不允许执行该操作", ErrorCodes.CustomerIsVoided);
        }

        if (customer.IsInSea)
        {
            return;
        }

        if (customer.OwnerId != operatorId)
        {
            throw new KnownException("仅领用人可作废该客户", ErrorCodes.CustomerNotInSea);
        }

        if (!customer.ClaimedAt.HasValue)
        {
            throw new KnownException("仅曾从公海领用的客户可由领用人作废", ErrorCodes.CustomerNotInSea);
        }
    }
}
