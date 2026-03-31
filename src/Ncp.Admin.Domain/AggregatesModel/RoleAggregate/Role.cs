using Ncp.Admin.Domain.DomainEvents.RoleEvents;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

/// <summary>
/// 数据权限范围
/// </summary>
public enum DataScope
{
    /// <summary>
    /// 全部数据
    /// </summary>
    All = 0,
    /// <summary>
    /// 本部门
    /// </summary>
    Dept = 1,
    /// <summary>
    /// 本部门及下级部门
    /// </summary>
    DeptAndSub = 2,
    /// <summary>
    /// 仅本人
    /// </summary>
    Self = 3,
    /// <summary>
    /// 自定义部门及下级部门（按角色配置部门集合）
    /// </summary>
    CustomDeptAndSub = 4,
}

public partial record RoleId : IGuidStronglyTypedId;

public class Role : Entity<RoleId>, IAggregateRoot
{
    protected Role()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    /// <summary>
    /// 数据权限范围
    /// </summary>
    public DataScope DataScope { get; private set; } = DataScope.All;
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsActive { get; private set; } = true;
    public Deleted IsDeleted { get; private set; } = new Deleted(false);
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    public virtual ICollection<RolePermission> Permissions { get; init; } = [];
    /// <summary>
    /// 自定义数据权限部门列表（当 DataScope 为 <see cref="DataScope.CustomDeptAndSub"/> 时生效）
    /// </summary>
    public virtual ICollection<RoleDataDept> DataDepts { get; init; } = [];

    public Role(string name, string description, IEnumerable<RolePermission> permissions, DataScope dataScope = DataScope.All)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        Description = description;
        DataScope = dataScope;
        Permissions = new List<RolePermission>(permissions);
        IsActive = true;
    }

    public void UpdateRoleInfo(string name, string description, DataScope? dataScope = null)
    {
        Name = name;
        Description = description;
        if (dataScope.HasValue)
            DataScope = dataScope.Value;
        AddDomainEvent(new RoleInfoChangedDomainEvent(this));
    }

    public void UpdateRolePermissions(IEnumerable<RolePermission> newPermissions)
    {
        var currentPermissionMap = Permissions.ToDictionary(p => p.PermissionCode);
        var newPermissionMap = newPermissions.ToDictionary(p => p.PermissionCode);

        var permissionsToRemove = currentPermissionMap.Keys.Except(newPermissionMap.Keys).ToList();
        foreach (var permissionCode in permissionsToRemove)
        {
            Permissions.Remove(currentPermissionMap[permissionCode]);
        }

        var permissionsToAdd = newPermissionMap.Keys.Except(currentPermissionMap.Keys).ToList();
        foreach (var permissionCode in permissionsToAdd)
        {
            Permissions.Add(newPermissionMap[permissionCode]);
        }

        // RolePermissionChangedDomainEvent 已定义于 RoleEvents.cs，当前无处理器；若将来需同步权限缓存可在此发布并添加处理器
    }

    public void SetCustomDataDepts(IEnumerable<DeptId> deptIds)
    {
        var distinct = (deptIds ?? Enumerable.Empty<DeptId>()).Distinct().ToList();
        DataDepts.Clear();
        foreach (var deptId in distinct)
        {
            DataDepts.Add(new RoleDataDept(Id, deptId));
        }
    }

    public void ClearCustomDataDepts()
    {
        DataDepts.Clear();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new KnownException("角色已经被停用", ErrorCodes.RoleAlreadyDeactivated);
        }

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new KnownException("角色已经是激活状态", ErrorCodes.RoleAlreadyActivated);
        }

        IsActive = true;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public bool HasPermission(string permissionCode)
    {
        return Permissions.Any(p => p.PermissionCode == permissionCode);
    }
}
