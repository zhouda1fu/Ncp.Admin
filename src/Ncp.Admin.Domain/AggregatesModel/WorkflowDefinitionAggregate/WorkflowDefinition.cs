using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;

namespace Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

/// <summary>
/// 流程定义ID（强类型ID）
/// </summary>
public partial record WorkflowDefinitionId : IGuidStronglyTypedId;

/// <summary>
/// 流程定义聚合根
/// 用于管理工作流模板的定义、版本和发布状态
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
    /// 流程定义JSON
    /// </summary>
    public string DefinitionJson { get; private set; } = string.Empty;

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
    /// 流程节点集合
    /// </summary>
    public virtual ICollection<WorkflowNode> Nodes { get; init; } = [];

    /// <summary>
    /// 创建流程定义（含节点，与 Role 构造函数模式一致）
    /// </summary>
    public WorkflowDefinition(string name, string description, string category, string definitionJson, UserId createdBy, IEnumerable<WorkflowNode> nodes)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        Description = description;
        Category = category;
        DefinitionJson = definitionJson;
        CreatedBy = createdBy;
        Status = WorkflowDefinitionStatus.Draft;
        Nodes = new List<WorkflowNode>(nodes);
    }

    /// <summary>
    /// 更新流程定义信息
    /// </summary>
    public void UpdateInfo(string name, string description, string category, string definitionJson, IEnumerable<WorkflowNode> nodes)
    {
        if (Status == WorkflowDefinitionStatus.Published)
        {
            throw new KnownException("已发布的流程定义不能修改，请创建新版本", ErrorCodes.WorkflowDefinitionAlreadyPublished);
        }

        Name = name;
        Description = description;
        Category = category;
        DefinitionJson = definitionJson;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

        // 更新节点
        Nodes.Clear();
        foreach (var node in nodes)
        {
            Nodes.Add(node);
        }

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
    /// 获取按排序的审批节点列表
    /// </summary>
    public IReadOnlyList<WorkflowNode> GetOrderedApprovalNodes()
    {
        return Nodes
            .Where(n => n.NodeType == WorkflowNodeType.Approval)
            .OrderBy(n => n.SortOrder)
            .ToList();
    }

    /// <summary>
    /// 获取第一个审批节点
    /// </summary>
    public WorkflowNode? GetFirstApprovalNode()
    {
        return GetOrderedApprovalNodes().FirstOrDefault();
    }

    /// <summary>
    /// 根据当前节点名称获取下一个审批节点
    /// </summary>
    public WorkflowNode? GetNextApprovalNode(string currentNodeName)
    {
        var orderedNodes = GetOrderedApprovalNodes();
        var currentIndex = orderedNodes.ToList().FindIndex(n => n.NodeName == currentNodeName);

        if (currentIndex >= 0 && currentIndex < orderedNodes.Count - 1)
        {
            return orderedNodes[currentIndex + 1];
        }

        return null; // 没有下一个节点，流程应结束
    }

    /// <summary>
    /// 创建新版本（基于当前定义）
    /// </summary>
    public WorkflowDefinition CreateNewVersion()
    {
        var clonedNodes = Nodes.Select(node => new WorkflowNode(
            node.NodeName,
            node.NodeType,
            node.AssigneeType,
            node.AssigneeValue,
            node.SortOrder,
            node.Description));

        var newDefinition = new WorkflowDefinition(Name, Description, Category, DefinitionJson, CreatedBy, clonedNodes)
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
