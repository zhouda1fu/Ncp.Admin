using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.Admin.Domain.DomainEvents.RoleEvents;

public record RoleInfoChangedDomainEvent(Role Role) : IDomainEvent;
public record RolePermissionChangedDomainEvent(Role Role) : IDomainEvent;

