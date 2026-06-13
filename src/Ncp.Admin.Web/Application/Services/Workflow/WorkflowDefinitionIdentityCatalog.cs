namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 流程定义导出文件中的身份目录，用于数据库重置后按名称重新匹配用户/角色/部门 ID。
/// </summary>
public sealed class WorkflowDefinitionIdentityCatalog
{
    public List<WorkflowDefinitionIdentityCatalogUserEntry> Users { get; set; } = [];

    public List<WorkflowDefinitionIdentityCatalogRoleEntry> Roles { get; set; } = [];

    public List<WorkflowDefinitionIdentityCatalogDeptEntry> Depts { get; set; } = [];

    /// <summary>流程节点摘要（nodeId 为设计器内稳定键，非数据库主键）。</summary>
    public List<WorkflowDefinitionIdentityCatalogNodeEntry> Nodes { get; set; } = [];
}

public sealed class WorkflowDefinitionIdentityCatalogUserEntry
{
    public string ExportedId { get; set; } = string.Empty;

    /// <summary>设计器中展示名（通常为真实姓名）。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>登录名（若导出时可解析则附带，便于重名时辅助匹配）。</summary>
    public string? AccountName { get; set; }
}

public sealed class WorkflowDefinitionIdentityCatalogRoleEntry
{
    public string ExportedId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public sealed class WorkflowDefinitionIdentityCatalogDeptEntry
{
    public string ExportedId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public sealed class WorkflowDefinitionIdentityCatalogNodeEntry
{
    public string NodeId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

/// <summary>导入时身份重映射结果。</summary>
public sealed class WorkflowDefinitionIdentityRemapResult
{
    public required string DesignerSchemaJson { get; init; }

    public WorkflowDefinitionIdentityRemapReport Report { get; init; } = new();

    public IReadOnlyList<string> Warnings => Report.Warnings;
}

public sealed class WorkflowDefinitionIdentityRemapReport
{
    public int UsersRemapped { get; set; }

    public int UsersKept { get; set; }

    public int UsersUnresolved { get; set; }

    public int RolesRemapped { get; set; }

    public int RolesKept { get; set; }

    public int RolesUnresolved { get; set; }

    public int DeptsRemapped { get; set; }

    public int DeptsKept { get; set; }

    public int DeptsUnresolved { get; set; }

    public List<string> Warnings { get; set; } = [];
}
