using Ncp.Admin.Domain;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 订单提交审批领域事件处理器：启动订单工作流并回写实例
/// </summary>
public class OrderSubmitRequestedDomainEventHandlerForStartWorkflow(
    WorkflowDefinitionQuery workflowDefinitionQuery,
    IMediator mediator)
    : IDomainEventHandler<OrderSubmitRequestedDomainEvent>
{
    public async Task Handle(OrderSubmitRequestedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var order = domainEvent.Order;

        var definitionDto = await workflowDefinitionQuery.GetFirstPublishedByCategoryAsync(
            WorkflowBusinessTypes.Order, cancellationToken)
            ?? throw new KnownException("未配置订单审批流程，请在流程定义中发布分类为「订单审批」的流程", ErrorCodes.OrderWorkflowNotConfigured);

        var variablesJson = OrderWorkflowVariablesBuilder.BuildJson(order);
        var title = $"订单审批-{order.OrderNumber}-{order.CustomerName}";
        var startCmd = new StartWorkflowCommand(
            definitionDto.Id,
            order.Id.ToString(),
            WorkflowBusinessTypes.Order,
            title,
            order.CreatorId,
            order.OwnerName,
            variablesJson,
            domainEvent.Remark);

        var instanceId = await mediator.Send(startCmd, cancellationToken);
        await mediator.Send(new ConfirmOrderWorkflowStartedCommand(order.Id, instanceId), cancellationToken);
    }
}
