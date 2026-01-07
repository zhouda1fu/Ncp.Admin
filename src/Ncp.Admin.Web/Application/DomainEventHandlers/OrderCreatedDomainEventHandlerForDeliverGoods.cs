using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Delivers;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

public class OrderCreatedDomainEventHandlerForDeliverGoods(IMediator mediator) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return mediator.Send(new DeliverGoodsCommand(notification.Order.Id), cancellationToken);
    }
}