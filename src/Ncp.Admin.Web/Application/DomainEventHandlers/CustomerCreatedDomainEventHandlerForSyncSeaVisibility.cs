using MediatR;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Customers;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 公海客户创建后，同步片区可见性并通知负责人。
/// </summary>
public class CustomerCreatedDomainEventHandlerForSyncSeaVisibility(IMediator mediator)
    : IDomainEventHandler<CustomerCreatedDomainEvent>
{
    public async Task Handle(CustomerCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var customer = domainEvent.Customer;
        if (!customer.IsInSea || customer.IsVoided)
            return;

        await mediator.Send(new SyncCustomerSeaVisibilityCommand(customer.Id), cancellationToken);
    }
}
