using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// 用户部门关系实体（与用户一对一）
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
    public DeptId DeptId { get; private set; } = DeptId.Unassigned;

    /// <summary>
    /// 部门名称
    /// </summary>
    public string DeptName { get; private set; } = string.Empty;

    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTimeOffset AssignedAt { get; init; }

    /// <summary>
    /// 创建用户部门关系
    /// </summary>
    /// <param name="deptId">部门ID</param>
    /// <param name="deptName">部门名称</param>
    internal UserDept(DeptId deptId, string deptName)
    {
        DeptId = deptId;
        AssignedAt = DateTimeOffset.UtcNow;
        DeptName = deptName;
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
