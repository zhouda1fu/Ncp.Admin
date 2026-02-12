using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

/// <summary>
/// 岗位ID（强类型ID）
/// </summary>
public partial record PositionId : IInt64StronglyTypedId;

/// <summary>
/// 岗位聚合根
/// 用于管理企业岗位（职位）信息
/// </summary>
public class Position : Entity<PositionId>, IAggregateRoot
{
    protected Position()
    {
    }

    /// <summary>
    /// 岗位名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 岗位编码（唯一标识，如 CEO、CTO、DEV）
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 岗位描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 所属部门ID
    /// </summary>
    public DeptId DeptId { get; private set; } = default!;

    /// <summary>
    /// 排序号（越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 状态（0=禁用，1=启用）
    /// </summary>
    public int Status { get; private set; } = 1;

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
    /// 创建岗位
    /// </summary>
    public Position(string name, string code, string description, DeptId deptId, int sortOrder, int status)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        Code = code;
        Description = description;
        DeptId = deptId;
        SortOrder = sortOrder;
        Status = status;
    }

    /// <summary>
    /// 更新岗位信息
    /// </summary>
    public void UpdateInfo(string name, string code, string description, DeptId deptId, int sortOrder, int status)
    {
        Name = name;
        Code = code;
        Description = description;
        DeptId = deptId;
        SortOrder = sortOrder;
        Status = status;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 激活岗位
    /// </summary>
    public void Activate()
    {
        if (Status == 1)
        {
            throw new KnownException("岗位已经是激活状态", ErrorCodes.PositionAlreadyActivated);
        }

        Status = 1;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 停用岗位
    /// </summary>
    public void Deactivate()
    {
        if (Status == 0)
        {
            throw new KnownException("岗位已经被停用", ErrorCodes.PositionAlreadyDeactivated);
        }

        Status = 0;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 软删除岗位
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted)
        {
            throw new KnownException("岗位已经被删除", ErrorCodes.PositionAlreadyDeleted);
        }

        IsDeleted = true;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
