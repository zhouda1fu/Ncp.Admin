using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Domain.DomainEvents.WorkflowEvents;

/// <summary>
/// 流程定义信息变更领域事件
/// </summary>
public record WorkflowDefinitionInfoChangedDomainEvent(WorkflowDefinition WorkflowDefinition) : IDomainEvent;

/// <summary>
/// 流程定义发布领域事件
/// </summary>
public record WorkflowDefinitionPublishedDomainEvent(WorkflowDefinition WorkflowDefinition) : IDomainEvent;

/// <summary>
/// 流程定义归档领域事件
/// </summary>
public record WorkflowDefinitionArchivedDomainEvent(WorkflowDefinition WorkflowDefinition) : IDomainEvent;

/// <summary>
/// 流程实例启动领域事件
/// </summary>
public record WorkflowInstanceStartedDomainEvent(WorkflowInstance WorkflowInstance) : IDomainEvent;

/// <summary>
/// 流程实例完成领域事件（审批通过）
/// </summary>
public record WorkflowInstanceCompletedDomainEvent(WorkflowInstance WorkflowInstance) : IDomainEvent;

/// <summary>
/// 流程实例驳回领域事件
/// </summary>
public record WorkflowInstanceRejectedDomainEvent(WorkflowInstance WorkflowInstance) : IDomainEvent;

/// <summary>
/// 工作流任务创建领域事件
/// </summary>
public record WorkflowTaskCreatedDomainEvent(WorkflowInstance WorkflowInstance, WorkflowTask WorkflowTask) : IDomainEvent;

/// <summary>
/// 工作流任务完成领域事件
/// </summary>
public record WorkflowTaskCompletedDomainEvent(WorkflowInstance WorkflowInstance, WorkflowTask WorkflowTask) : IDomainEvent;
