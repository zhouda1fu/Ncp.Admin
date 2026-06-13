namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 工作流业务类型常量（BusinessType）。
/// </summary>
public static class WorkflowBusinessTypes
{
    /// <summary>创建用户审批（平台示范流程）。</summary>
    public const string CreateUser = nameof(CreateUser);

    /// <summary>流程设计器条件分支是否提供「路由角色」字段。</summary>
    public static bool SupportsWorkflowRoutingRoleConditionField(string? category) =>
        string.Equals(category, CreateUser, StringComparison.Ordinal);
}
