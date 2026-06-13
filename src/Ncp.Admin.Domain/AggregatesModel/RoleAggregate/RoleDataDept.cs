using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

public class RoleDataDept
{
    private RoleDataDept()
    {
    }

    internal RoleDataDept(DeptId deptId)
    {
        DeptId = deptId;
    }

    public RoleId RoleId { get; private set; } = RoleId.Unassigned;
    public DeptId DeptId { get; private set; } = DeptId.Unassigned;
}

