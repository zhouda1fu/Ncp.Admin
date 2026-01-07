using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

public record OrderCreatedDomainEvent(Order Order) : IDomainEvent;

public record OrderPaidDomainEvent(Order Order) : IDomainEvent;
