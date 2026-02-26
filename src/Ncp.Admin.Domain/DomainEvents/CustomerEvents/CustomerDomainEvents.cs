using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

namespace Ncp.Admin.Domain.DomainEvents.CustomerEvents;

/// <summary>
/// 客户已创建。
/// </summary>
public record CustomerCreatedDomainEvent(Customer Customer) : IDomainEvent;

/// <summary>
/// 客户已更新。
/// </summary>
public record CustomerUpdatedDomainEvent(Customer Customer) : IDomainEvent;

/// <summary>
/// 客户已释放到公海。
/// </summary>
public record CustomerReleasedToSeaDomainEvent(Customer Customer) : IDomainEvent;

/// <summary>
/// 客户已从公海领用。
/// </summary>
public record CustomerClaimedFromSeaDomainEvent(Customer Customer) : IDomainEvent;

/// <summary>
/// 客户联系人已添加。
/// </summary>
public record CustomerContactAddedDomainEvent(Customer Customer, CustomerContact Contact) : IDomainEvent;

/// <summary>
/// 客户联系人已更新。
/// </summary>
public record CustomerContactUpdatedDomainEvent(Customer Customer, CustomerContact Contact) : IDomainEvent;

/// <summary>
/// 客户联系人已移除。
/// </summary>
public record CustomerContactRemovedDomainEvent(Customer Customer, CustomerContactId ContactId) : IDomainEvent;
