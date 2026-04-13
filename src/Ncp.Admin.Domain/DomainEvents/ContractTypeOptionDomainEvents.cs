using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 合同类型选项已创建
/// </summary>
public record ContractTypeOptionCreatedDomainEvent(ContractTypeOption Option) : IDomainEvent;

/// <summary>
/// 合同类型选项已更新
/// </summary>
public record ContractTypeOptionUpdatedDomainEvent(ContractTypeOption Option) : IDomainEvent;
