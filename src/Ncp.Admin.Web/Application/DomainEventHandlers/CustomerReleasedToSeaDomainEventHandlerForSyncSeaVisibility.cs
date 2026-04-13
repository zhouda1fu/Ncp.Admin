using MediatR;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Customers;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 客户释放到公海后，按区域重新计算片区可见性。
/// </summary>
public class CustomerReleasedToSeaDomainEventHandlerForSyncSeaVisibility(IMediator mediator)
    : IDomainEventHandler<CustomerReleasedToSeaDomainEvent>
{
    public async Task Handle(CustomerReleasedToSeaDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var customer = domainEvent.Customer;
        if (customer.IsVoided)
            return;

        await mediator.Send(new SyncCustomerSeaVisibilityCommand(customer.Id), cancellationToken);
    }
}
