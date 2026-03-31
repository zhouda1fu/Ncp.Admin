using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

/// <summary>
/// 流程定义ID（强类型ID）
/// </summary>
public partial record WorkflowDefinitionId : IGuidStronglyTypedId;

/// <summary>
/// 流程定义聚合根
/// 用于管理工作流模板的定义、版本和发布状态。流程结构存储在 DefinitionJson（设计器树形 JSON）中。
/// </summary>
public class WorkflowDefinition : Entity<WorkflowDefinitionId>, IAggregateRoot
{
    protected WorkflowDefinition()
    {
    }

    /// <summary>
    /// 流程名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 流程描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public int Version { get; private set; } = 1;

    /// <summary>
    /// 流程分类（如：请假审批、采购审批）
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// 流程状态
    /// </summary>
    public WorkflowDefinitionStatus Status { get; private set; } = WorkflowDefinitionStatus.Draft;

    /// <summary>
    /// 流程定义JSON（设计器树形结构）
    /// </summary>
    public string DefinitionJson { get; private set; } = string.Empty;

    /// <summary>
    /// 基于哪条流程定义创建（通过「基于此创建新版本」产生时为源定义ID，否则为 Guid.Empty）
    /// </summary>
    public WorkflowDefinitionId BasedOnId { get; private set; } = new WorkflowDefinitionId(Guid.Empty);

    /// <summary>
    /// 创建人ID
    /// </summary>
    public UserId CreatedBy { get; private set; } = default!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 是否删除
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 删除时间
    /// </summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建流程定义
    /// </summary>
    /// <param name="basedOnId">基于哪条流程定义创建，仅「基于此创建新版本」时传入</param>
    public WorkflowDefinition(string name, string description, string category, string definitionJson, UserId createdBy, WorkflowDefinitionId? basedOnId = null)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        Description = description;
        Category = category;
        DefinitionJson = definitionJson ?? string.Empty;
        CreatedBy = createdBy;
        BasedOnId = basedOnId ?? new WorkflowDefinitionId(Guid.Empty);
        Status = WorkflowDefinitionStatus.Draft;
    }

    /// <summary>
    /// 更新流程定义信息
    /// </summary>
    public void UpdateInfo(string name, string description, string category, string definitionJson)
    {
        if (Status == WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("已发布的流程定义不能修改，请创建新版本", ErrorCodes.WorkflowDefinitionAlreadyPublished);
        }

        Name = name;
        Description = description;
        Category = category;
        DefinitionJson = definitionJson ?? string.Empty;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

        AddDomainEvent(new WorkflowDefinitionInfoChangedDomainEvent(this));
    }

    /// <summary>
    /// 发布流程定义
    /// </summary>
    public void Publish()
    {
        if (Status == WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("流程定义已经发布", ErrorCodes.WorkflowDefinitionAlreadyPublished);
        }

        if (Status == WorkflowDefinitionStatus.Archived)
        {
            throw new KnownException("已归档的流程定义不能发布", ErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        Status = WorkflowDefinitionStatus.Published;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new WorkflowDefinitionPublishedDomainEvent(this));
    }

    /// <summary>
    /// 归档流程定义
    /// </summary>
    public void Archive()
    {
        if (Status == WorkflowDefinitionStatus.Archived)
        {
            throw new KnownException("流程定义已经归档", ErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        Status = WorkflowDefinitionStatus.Archived;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new WorkflowDefinitionArchivedDomainEvent(this));
    }

    /// <summary>
    /// 创建新版本（基于当前定义），新定义的 BasedOnId 指向当前聚合，发布新版本时可据此归档当前定义
    /// </summary>
    public WorkflowDefinition CreateNewVersion()
    {
        var newDefinition = new WorkflowDefinition(Name, Description, Category, DefinitionJson, CreatedBy, Id)
        {
            Version = Version + 1
        };
        return newDefinition;
    }

    /// <summary>
    /// 软删除
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted)
        {
            throw new KnownException("流程定义已经被删除", ErrorCodes.WorkflowDefinitionAlreadyDeleted);
        }

        IsDeleted = true;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}

/// <summary>
/// 流程定义状态枚举
/// </summary>
public enum WorkflowDefinitionStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已发布
    /// </summary>
    Published = 1,

    /// <summary>
    /// 已归档
    /// </summary>
    Archived = 2
}
