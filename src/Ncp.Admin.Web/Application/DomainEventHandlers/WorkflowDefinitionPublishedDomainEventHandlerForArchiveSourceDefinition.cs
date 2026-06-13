using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Workflows;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 流程定义发布领域事件处理器，用于归档新版本来源定义。
/// </summary>
public class WorkflowDefinitionPublishedDomainEventHandlerForArchiveSourceDefinition(IMediator mediator)
    : IDomainEventHandler<WorkflowDefinitionPublishedDomainEvent>
{
    public async Task Handle(WorkflowDefinitionPublishedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var sourceDefinitionId = domainEvent.WorkflowDefinition.BasedOnId;
        if (sourceDefinitionId == WorkflowDefinitionId.Unassigned)
        {
            return;
        }

        await mediator.Send(new ArchiveWorkflowDefinitionCommand(sourceDefinitionId), cancellationToken);
    }
}
