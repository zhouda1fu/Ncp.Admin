using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 合同已创建（草稿）
/// </summary>
public record ContractCreatedDomainEvent(Contract Contract) : IDomainEvent;

/// <summary>
/// 合同主数据或发票明细已变更（仍由聚合根统一持久化）
/// </summary>
public record ContractUpdatedDomainEvent(Contract Contract) : IDomainEvent;

/// <summary>
/// 合同已提交审批
/// </summary>
public record ContractSubmitForApprovalDomainEvent(Contract Contract) : IDomainEvent;

/// <summary>
/// 合同审批已通过
/// </summary>
public record ContractApprovedDomainEvent(Contract Contract) : IDomainEvent;

/// <summary>
/// 合同已归档
/// </summary>
public record ContractArchivedDomainEvent(Contract Contract) : IDomainEvent;

/// <summary>
/// 合同已标记删除（软删，仅草稿可触发）
/// </summary>
public record ContractDeletedDomainEvent(Contract Contract) : IDomainEvent;
