using System;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

/// <summary>
/// 流程定义ID（强类型ID）
/// </summary>
public partial record WorkflowDefinitionId : IGuidStronglyTypedId
{
    /// <summary>
    /// 未分配标识（哨兵值）
    /// </summary>
    public static WorkflowDefinitionId Unassigned { get; } = new(Guid.Empty);
}

/// <summary>
/// 流程定义聚合根
/// 用于管理工作流模板的定义、版本和发布状态。流程结构存储在 DesignerSchemaJson（当前设计器 JSON）中。
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
    /// 设计器 Schema JSON
    /// </summary>
    public string DesignerSchemaJson { get; private set; } = string.Empty;

    /// <summary>
    /// 基于哪条流程定义创建（通过「基于此创建新版本」产生时为源定义ID，否则为 Guid.Empty）
    /// </summary>
    public WorkflowDefinitionId BasedOnId { get; private set; } = WorkflowDefinitionId.Unassigned;

    /// <summary>
    /// 创建人ID
    /// </summary>
    public UserId CreatedBy { get; private set; } = UserId.Unassigned;

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
    /// 流程定义版本集合。
    /// </summary>
    public virtual ICollection<WorkflowDefinitionVersion> Versions { get; } = [];

    /// <summary>
    /// 创建流程定义（非基于已有定义复制）
    /// </summary>
    public WorkflowDefinition(string name, string description, string category, string designerSchemaJson, UserId createdBy)
        : this(name, description, category, designerSchemaJson, createdBy, new WorkflowDefinitionId(Guid.Empty))
    {
    }

    /// <summary>
    /// 创建流程定义
    /// </summary>
    /// <param name="basedOnId">基于哪条流程定义创建，仅「基于此创建新版本」时传入</param>
    public WorkflowDefinition(string name, string description, string category, string designerSchemaJson, UserId createdBy, WorkflowDefinitionId basedOnId)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        Description = description;
        Category = category;
        DesignerSchemaJson = designerSchemaJson;
        CreatedBy = createdBy;
        BasedOnId = basedOnId;
        Status = WorkflowDefinitionStatus.Draft;
    }

    /// <summary>
    /// 更新流程定义信息
    /// </summary>
    public void UpdateInfo(string name, string description, string category, string designerSchemaJson)
    {
        if (Status == WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("已发布的流程定义不能修改，请创建新版本", ErrorCodes.WorkflowDefinitionAlreadyPublished);
        }

        Name = name;
        Description = description;
        Category = category;
        DesignerSchemaJson = designerSchemaJson;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

        AddDomainEvent(new WorkflowDefinitionInfoChangedDomainEvent(this));
    }

    /// <summary>
    /// 添加草稿版本。
    /// </summary>
    public WorkflowDefinitionVersion AddDraftVersion(string designerSchemaJson)
    {
        var nextVersion = Versions.Count == 0 ? Version : Versions.Max(v => v.Version) + 1;
        var version = new WorkflowDefinitionVersion(Id, nextVersion, designerSchemaJson);
        Versions.Add(version);
        return version;
    }

    /// <summary>
    /// 获取最新版本。
    /// </summary>
    public WorkflowDefinitionVersion? GetLatestVersion()
    {
        return Versions.OrderByDescending(v => v.Version).FirstOrDefault();
    }

    /// <summary>
    /// 获取最新已发布版本。
    /// </summary>
    public WorkflowDefinitionVersion? GetLatestPublishedVersion()
    {
        return Versions
            .Where(v => v.Status == WorkflowDefinitionVersionStatus.Published)
            .OrderByDescending(v => v.Version)
            .FirstOrDefault();
    }

    /// <summary>
    /// 获取最新草稿版本；不存在时创建。
    /// </summary>
    public WorkflowDefinitionVersion EnsureDraftVersion()
    {
        var latest = GetLatestVersion();
        if (latest is { Status: WorkflowDefinitionVersionStatus.Draft })
        {
            return latest;
        }

        return AddDraftVersion(DesignerSchemaJson);
    }

    /// <summary>
    /// 更新最新草稿版本。
    /// </summary>
    public void UpdateLatestDraftVersion(string designerSchemaJson)
    {
        EnsureDraftVersion().UpdateDraftSchema(designerSchemaJson);
    }

    /// <summary>
    /// 发布最新草稿版本。
    /// </summary>
    public WorkflowDefinitionVersion PublishLatestDraftVersion(string graphSnapshotJson, UserId publishedBy)
    {
        var draft = EnsureDraftVersion();
        draft.Publish(graphSnapshotJson, publishedBy);
        return draft;
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
        var newDefinition = new WorkflowDefinition(Name, Description, Category, DesignerSchemaJson, CreatedBy, Id)
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
