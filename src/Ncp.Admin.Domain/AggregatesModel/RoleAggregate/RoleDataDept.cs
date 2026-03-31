using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

public class RoleDataDept
{
    private RoleDataDept()
    {
    }

    public RoleDataDept(RoleId roleId, DeptId deptId)
    {
        RoleId = roleId;
        DeptId = deptId;
    }

    public RoleId RoleId { get; private set; } = default!;
    public DeptId DeptId { get; private set; } = default!;
}

