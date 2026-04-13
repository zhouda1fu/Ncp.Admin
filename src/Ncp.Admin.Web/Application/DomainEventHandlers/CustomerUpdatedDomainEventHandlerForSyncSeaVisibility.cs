using MediatR;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Customers;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 公海未领用客户信息变更时，重新同步片区可见性（区域变化会撤回旧负责人并通知）。
/// </summary>
public class CustomerUpdatedDomainEventHandlerForSyncSeaVisibility(IMediator mediator)
    : IDomainEventHandler<CustomerUpdatedDomainEvent>
{
    public async Task Handle(CustomerUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var customer = domainEvent.Customer;
        if (!customer.IsInSea || customer.IsVoided)
            return;

        await mediator.Send(new SyncCustomerSeaVisibilityCommand(customer.Id), cancellationToken);
    }
}
