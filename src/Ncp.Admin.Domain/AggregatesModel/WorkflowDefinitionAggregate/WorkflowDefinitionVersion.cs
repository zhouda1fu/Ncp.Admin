using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

/// <summary>
/// 流程定义版本ID（强类型ID）
/// </summary>
public partial record WorkflowDefinitionVersionId : IGuidStronglyTypedId
{
    /// <summary>
    /// 未分配标识（哨兵值）
    /// </summary>
    public static WorkflowDefinitionVersionId Unassigned { get; } = new(Guid.Empty);
}

/// <summary>
/// 流程定义版本，保存某一次草稿编辑内容与发布后的运行图快照。
/// </summary>
public class WorkflowDefinitionVersion : Entity<WorkflowDefinitionVersionId>
{
    protected WorkflowDefinitionVersion()
    {
    }

    /// <summary>
    /// 所属流程定义ID
    /// </summary>
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; } = WorkflowDefinitionId.Unassigned;

    /// <summary>
    /// 版本号
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// 版本状态
    /// </summary>
    public WorkflowDefinitionVersionStatus Status { get; private set; } = WorkflowDefinitionVersionStatus.Draft;

    /// <summary>
    /// 前端设计器 JSON。
    /// </summary>
    public string DesignerSchemaJson { get; private set; } = string.Empty;

    /// <summary>
    /// 发布后运行图快照 JSON。
    /// </summary>
    public string GraphSnapshotJson { get; private set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new(DateTimeOffset.UtcNow);

    /// <summary>
    /// 发布人
    /// </summary>
    public UserId PublishedBy { get; private set; } = UserId.Unassigned;

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTimeOffset PublishedAt { get; private set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// 创建流程定义版本
    /// </summary>
    public WorkflowDefinitionVersion(
        WorkflowDefinitionId workflowDefinitionId,
        int version,
        string designerSchemaJson)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        WorkflowDefinitionId = workflowDefinitionId;
        Version = version;
        DesignerSchemaJson = designerSchemaJson;
        Status = WorkflowDefinitionVersionStatus.Draft;
    }

    /// <summary>
    /// 更新草稿设计器 JSON。
    /// </summary>
    public void UpdateDraftSchema(string designerSchemaJson)
    {
        if (Status != WorkflowDefinitionVersionStatus.Draft)
        {
            throw new KnownException("只有草稿版本可以修改", ErrorCodes.WorkflowDefinitionAlreadyPublished);
        }

        DesignerSchemaJson = designerSchemaJson;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 发布当前版本。
    /// </summary>
    public void Publish(string graphSnapshotJson, UserId publishedBy)
    {
        if (Status == WorkflowDefinitionVersionStatus.Published)
        {
            throw new KnownException("流程定义版本已经发布", ErrorCodes.WorkflowDefinitionAlreadyPublished);
        }

        if (Status == WorkflowDefinitionVersionStatus.Archived)
        {
            throw new KnownException("已归档的流程定义版本不能发布", ErrorCodes.WorkflowDefinitionAlreadyArchived);
        }

        GraphSnapshotJson = graphSnapshotJson;
        PublishedBy = publishedBy;
        PublishedAt = DateTimeOffset.UtcNow;
        Status = WorkflowDefinitionVersionStatus.Published;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 归档当前版本。
    /// </summary>
    public void Archive()
    {
        if (Status == WorkflowDefinitionVersionStatus.Archived)
        {
            return;
        }

        Status = WorkflowDefinitionVersionStatus.Archived;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}

/// <summary>
/// 流程定义版本状态
/// </summary>
public enum WorkflowDefinitionVersionStatus
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
