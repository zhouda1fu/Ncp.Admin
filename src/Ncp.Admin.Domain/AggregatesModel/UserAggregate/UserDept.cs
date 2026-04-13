using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// 用户部门关系实体（与用户一对一；主键同用户 ID）
/// </summary>
public class UserDept : Entity<UserId>
{
    /// <summary>
    /// EF/序列化用
    /// </summary>
    protected UserDept()
    {
    }

    /// <summary>
    /// 部门ID
    /// </summary>
    public DeptId DeptId { get; private set; } = default!;

    /// <summary>
    /// 部门名称
    /// </summary>
    public string DeptName { get; private set; } = string.Empty;

    /// <summary>
    /// 是否为该部门主管
    /// </summary>
    public bool IsDeptManager { get; private set; }

    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTimeOffset AssignedAt { get; init; }

    /// <summary>
    /// 创建用户部门关系（仅领域层使用；由 <see cref="User"/> 分配部门时构造，主键与用户聚合根一致）
    /// </summary>
    /// <param name="userId">用户ID（与聚合根一致）</param>
    /// <param name="deptId">部门ID</param>
    /// <param name="deptName">部门名称</param>
    /// <param name="isDeptManager">是否为该部门主管</param>
    internal UserDept(UserId userId, DeptId deptId, string deptName, bool isDeptManager = false)
    {
        Id = userId;
        DeptId = deptId;
        AssignedAt = DateTimeOffset.UtcNow;
        DeptName = deptName;
        IsDeptManager = isDeptManager;
    }

    /// <summary>
    /// 更新部门名称
    /// </summary>
    /// <param name="deptName">新的部门名称</param>
    public void UpdateDeptName(string deptName)
    {
        if (string.IsNullOrWhiteSpace(deptName))
        {
            throw new KnownException("部门名称不能为空", ErrorCodes.DeptNameCannotBeEmpty);
        }

        DeptName = deptName;
    }
}
