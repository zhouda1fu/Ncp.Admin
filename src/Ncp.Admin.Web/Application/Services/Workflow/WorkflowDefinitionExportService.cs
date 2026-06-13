namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 流程定义导出文档构建（含身份名称目录，便于库重置后按名称重映射）。
/// </summary>
public class WorkflowDefinitionExportService(WorkflowDefinitionIdentityCatalogBuilder catalogBuilder)
{
    public const string Format = "ncp-workflow-definition-export";

    /// <summary>当前导出格式版本（含 identityCatalog）。</summary>
    public const int CurrentVersion = 2;

    /// <summary>旧版导出格式（仅 definition，无 identityCatalog）。</summary>
    public const int LegacyVersion = 1;

    public WorkflowDefinitionExportDocument Build(
        string name,
        string description,
        string category,
        string designerSchemaJson,
        DateTimeOffset? exportedAt = null)
    {
        var catalog = catalogBuilder.Build(designerSchemaJson, category);
        return new WorkflowDefinitionExportDocument
        {
            Format = Format,
            Version = CurrentVersion,
            ExportedAt = (exportedAt ?? DateTimeOffset.UtcNow).ToString("O"),
            RemapStrategy = "byName",
            Definition = new WorkflowDefinitionExportDefinitionPayload
            {
                Name = name,
                Description = description,
                Category = category,
                DesignerSchemaJson = designerSchemaJson,
            },
            IdentityCatalog = catalog,
        };
    }
}

public sealed class WorkflowDefinitionExportDocument
{
    public string Format { get; set; } = WorkflowDefinitionExportService.Format;

    public int Version { get; set; } = WorkflowDefinitionExportService.CurrentVersion;

    public string ExportedAt { get; set; } = string.Empty;

    /// <summary>身份重映射策略说明：byName = 导入时按名称匹配当前库中的用户/角色/部门。</summary>
    public string RemapStrategy { get; set; } = "byName";

    public WorkflowDefinitionExportDefinitionPayload Definition { get; set; } = null!;

    public WorkflowDefinitionIdentityCatalog? IdentityCatalog { get; set; }
}

public sealed class WorkflowDefinitionExportDefinitionPayload
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string DesignerSchemaJson { get; set; } = string.Empty;
}
