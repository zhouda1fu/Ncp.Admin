using System.ComponentModel.DataAnnotations.Schema;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

/// <summary>
/// 部门ID（强类型ID）
/// </summary>
public partial record DeptId : IInt64StronglyTypedId
{
    /// <summary>
    /// 未分配部门（哨兵值）
    /// </summary>
    public static DeptId Unassigned { get; } = new(0);
}

/// <summary>
/// 部门聚合根
/// 用于管理企业部门的层级结构
/// </summary>
public class Dept : Entity<DeptId>, IAggregateRoot
{
    /// <summary>
    /// 部门名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>
    /// 上级部门ID
    /// </summary>
    public DeptId ParentId { get; private set; } = DeptId.Unassigned;

    /// <summary>
    /// 状态（0=禁用，1=启用）
    /// </summary>
    public int Status { get; private set; } = 1;

    /// <summary>
    /// 排序号（同级部门内数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 删除时间
    /// </summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 并发版本
    /// </summary>
    public RowVersion RowVersion { get; private set; } = new RowVersion(0);

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 子部门（不映射到数据库，用于内存中的树形结构）
    /// </summary>
    [NotMapped]
    public virtual ICollection<Dept> Children { get; } = [];

    /// <summary>
    /// 部门负责人列表；部门聚合负责维护该集合的一致性，应用层只表达“替换为哪些负责人”。
    /// </summary>
    public ICollection<DeptResponsibleUser> ResponsibleUsers { get; private set; } = [];

    protected Dept()
    {
    }

    /// <summary>
    /// 创建部门
    /// </summary>
    /// <param name="name">部门名称</param>
    /// <param name="remark">备注</param>
    /// <param name="parentId">上级部门ID</param>
    /// <param name="status">状态（0=禁用，1=启用）</param>
    /// <param name="sortOrder">排序号</param>
    public Dept(string name, string remark, DeptId parentId, int status, int sortOrder = 0)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        Remark = remark;
        ParentId = parentId;
        Status = status;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新部门信息
    /// </summary>
    /// <param name="name">部门名称</param>
    /// <param name="remark">备注</param>
    /// <param name="parentId">上级部门ID</param>
    /// <param name="status">状态（0=禁用，1=启用）</param>
    /// <param name="sortOrder">排序号</param>
    public void UpdateInfo(string name, string remark, DeptId parentId, int status, int sortOrder = 0)
    {
        Name = name;
        Remark = remark;
        ParentId = parentId;
        Status = status;
        SortOrder = sortOrder;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

        AddDomainEvent(new DeptInfoChangedDomainEvent(this));
    }

    /// <summary>
    /// 更新同级排序号
    /// </summary>
    /// <param name="sortOrder">排序号</param>
    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 全量替换部门负责人，保证排重、排序和默认负责人约束都由部门聚合统一维护。
    /// </summary>
    /// <param name="responsibleUserIds">负责人用户 ID 列表。</param>
    /// <param name="defaultResponsibleUserId">默认负责人用户 ID；仅用于单人兜底场景。</param>
    public void ReplaceResponsibleUsers(
        IReadOnlyList<UserId> responsibleUserIds,
        UserId? defaultResponsibleUserId)
    {
        var normalized = responsibleUserIds
            .Where(id => id != UserId.Unassigned)
            .Distinct()
            .ToList();
        var defaultId = defaultResponsibleUserId is { } d && d != UserId.Unassigned ? d : null;
        if (defaultId != null && !normalized.Contains(defaultId))
        {
            throw new KnownException("默认负责人必须在部门负责人列表中", ErrorCodes.DeptResponsibleUserDefaultInvalid);
        }

        ResponsibleUsers.Clear();
        foreach (var (userId, index) in normalized.Select((userId, index) => (userId, index)))
        {
            // 子实体始终由部门聚合创建，避免应用层绕过排序和默认负责人规则。
            ResponsibleUsers.Add(new DeptResponsibleUser(
                Id,
                userId,
                defaultId != null && userId == defaultId,
                index + 1));
        }

        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 追加部门负责人，并在需要时把该负责人设为默认负责人。
    /// 用于新增用户等快捷入口，实际负责人关系仍统一归部门聚合维护。
    /// </summary>
    /// <param name="userId">要追加为部门负责人的用户 ID。</param>
    /// <param name="setAsDefault">是否同步设为默认负责人。</param>
    public void AddResponsibleUser(UserId userId, bool setAsDefault)
    {
        SetResponsibleUser(userId, true, setAsDefault);
    }

    /// <summary>
    /// 设置单个用户在当前部门的负责人状态。
    /// 可用于编辑用户时同步负责人/默认负责人状态，避免重复追加时保留旧默认状态。
    /// </summary>
    /// <param name="userId">要设置的用户 ID。</param>
    /// <param name="setAsResponsible">是否设为部门负责人。</param>
    /// <param name="setAsDefault">是否设为默认负责人。</param>
    public void SetResponsibleUser(UserId userId, bool setAsResponsible, bool setAsDefault)
    {
        if (userId == UserId.Unassigned)
        {
            return;
        }

        var others = ResponsibleUsers
            .OrderBy(r => r.SortOrder)
            .Where(r => r.UserId != userId)
            .ToList();
        var responsibleUserIds = ResponsibleUsers
            .OrderBy(r => r.SortOrder)
            .Where(r => r.UserId != userId)
            .Select(r => r.UserId)
            .ToList();
        if (setAsResponsible)
        {
            responsibleUserIds.Add(userId);
        }

        // 当当前用户取消默认负责人时，不沿用其旧默认状态，只保留其他人的默认负责人。
        var defaultResponsibleUserId = setAsDefault
            ? userId
            : others.FirstOrDefault(r => r.IsDefault)?.UserId;
        ReplaceResponsibleUsers(responsibleUserIds, defaultResponsibleUserId);
    }

    /// <summary>
    /// 激活部门
    /// </summary>
    public void Activate()
    {
        if (Status == 1)
        {
            throw new KnownException("部门已经是激活状态", ErrorCodes.DeptAlreadyActivated);
        }

        Status = 1;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 停用部门
    /// </summary>
    public void Deactivate()
    {
        if (Status == 0)
        {
            throw new KnownException("部门已经被停用", ErrorCodes.DeptAlreadyDeactivated);
        }

        Status = 0;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 软删除部门
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted)
        {
            throw new KnownException("部门已经被删除", ErrorCodes.DeptAlreadyDeleted);
        }

        IsDeleted = true;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 添加子部门
    /// </summary>
    /// <param name="child">子部门</param>
    public void AddChild(Dept child)
    {
        if (child == null)
        {
            throw new KnownException("子部门不能为空", ErrorCodes.ChildDeptCannotBeEmpty);
        }

        Children.Add(child);
    }

    /// <summary>
    /// 移除子部门
    /// </summary>
    /// <param name="child">子部门</param>
    public void RemoveChild(Dept child)
    {
        if (child == null)
        {
            throw new KnownException("子部门不能为空", ErrorCodes.ChildDeptCannotBeEmpty);
        }

        Children.Remove(child);
    }

    /// <summary>
    /// 获取所有子部门（包括子级的子级）
    /// </summary>
    /// <returns>所有子部门</returns>
    public IEnumerable<Dept> GetAllChildren()
    {
        var result = new List<Dept>();
        foreach (var child in Children)
        {
            result.Add(child);
            result.AddRange(child.GetAllChildren());
        }
        return result;
    }

    /// <summary>
    /// 获取部门层级路径
    /// </summary>
    /// <returns>层级路径</returns>
    public string GetPath()
    {
        return Name;
    }
}

/// <summary>
/// 部门负责人关系ID（强类型ID）。
/// </summary>
public partial record DeptResponsibleUserId : IInt64StronglyTypedId
{
    public static DeptResponsibleUserId Unassigned { get; } = new(0);
}

/// <summary>
/// 部门负责人关系。
/// 一个部门可以配置多个负责人，用于工作流“部门负责人”审批来源，不再表达单一直属上级。
/// </summary>
public class DeptResponsibleUser : Entity<DeptResponsibleUserId>
{
    protected DeptResponsibleUser()
    {
    }

    /// <summary>
    /// 负责人所属部门。
    /// </summary>
    public DeptId DeptId { get; private set; } = DeptId.Unassigned;

    /// <summary>
    /// 负责人用户。
    /// </summary>
    public UserId UserId { get; private set; } = UserId.Unassigned;

    /// <summary>
    /// 是否为默认负责人；仅用于需要单人兜底的场景，不代表唯一上级。
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// 负责人解析顺序；工作流顺序审批会按该字段稳定排序。
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 创建部门负责人关系。
    /// </summary>
    public DeptResponsibleUser(DeptId deptId, UserId userId, bool isDefault, int sortOrder)
    {
        DeptId = deptId;
        UserId = userId;
        IsDefault = isDefault;
        SortOrder = sortOrder;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
