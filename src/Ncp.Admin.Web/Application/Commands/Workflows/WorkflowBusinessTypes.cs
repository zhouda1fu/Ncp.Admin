namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 工作流业务类型常量（BusinessType），集中管理所有与工作流关联的业务分类标识
/// </summary>
public static class WorkflowBusinessTypes
{
    public const string CreateUser = "CreateUser";
    public const string LeaveRequest = "LeaveRequest";
    public const string Order = "Order";

    /// <summary>客户作废审批（流程定义 Category 常量与此一致，界面展示名「客户作废」）</summary>
    public const string CustomerSeaVoid = "CustomerSeaVoid";
}
