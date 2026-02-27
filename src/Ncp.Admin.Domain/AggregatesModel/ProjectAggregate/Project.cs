using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

/// <summary>
/// 项目ID（强类型ID）
/// </summary>
public partial record ProjectId : IGuidStronglyTypedId;

/// <summary>
/// 项目状态
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// 进行中
    /// </summary>
    Active = 0,
    /// <summary>
    /// 已归档
    /// </summary>
    Archived = 1,
}

/// <summary>
/// 项目聚合根，用于任务看板与协作
/// </summary>
public class Project : Entity<ProjectId>, IAggregateRoot
{
    protected Project() { }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 项目描述（可选）
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;
    /// <summary>
    /// 项目状态
    /// </summary>
    public ProjectStatus Status { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建项目
    /// </summary>
    public Project(string name, UserId creatorId, string? description = null)
    {
        Name = name ;
        CreatorId = creatorId;
        Description = description;
        Status = ProjectStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新项目信息
    /// </summary>
    public void Update(string name, string? description)
    {
        Name = name ;
        Description = description;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 归档项目
    /// </summary>
    public void Archive()
    {
        if (Status == ProjectStatus.Archived)
            return;
        Status = ProjectStatus.Archived;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 激活项目
    /// </summary>
    public void Activate()
    {
        if (Status == ProjectStatus.Active)
            return;
        Status = ProjectStatus.Active;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
