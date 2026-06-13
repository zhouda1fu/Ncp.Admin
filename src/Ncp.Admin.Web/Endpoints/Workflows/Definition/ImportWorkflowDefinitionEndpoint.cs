using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.Definition;

/// <summary>导出文件中的 definition 段</summary>
public class ImportWorkflowDefinitionPayload
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DesignerSchemaJson { get; set; } = string.Empty;
}

/// <summary>与前端导出 JSON 根对象一致</summary>
public class ImportWorkflowDefinitionRequest
{
    public string Format { get; set; } = string.Empty;
    public int Version { get; set; }
    public ImportWorkflowDefinitionPayload Definition { get; set; } = null!;
    public bool UpsertByName { get; set; } = true;
}

public record ImportWorkflowDefinitionResponse(
    WorkflowDefinitionId Id,
    string Name,
    ImportWorkflowDefinitionAction Action,
    WorkflowDefinitionIdentityRemapReport RemapReport,
    IReadOnlyList<string> Warnings);

/// <summary>
/// 从导出 JSON 导入流程定义（按名称重映射身份 ID，默认按名称+分类更新草稿）
/// POST /api/admin/workflow/definitions/import
/// </summary>
public class ImportWorkflowDefinitionEndpoint(IMediator mediator)
    : Endpoint<ImportWorkflowDefinitionRequest, ResponseData<ImportWorkflowDefinitionResponse>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions").WithSummary("从导出文件导入流程定义（按名称重映射）"));
        Post("/api/admin/workflow/definitions/import");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.WorkflowDefinitionCreate);
    }

    public override async Task HandleAsync(ImportWorkflowDefinitionRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        if (req.Definition is null)
        {
            throw new KnownException("导入文件缺少 definition 节点", ErrorCodes.WorkflowDefinitionImportInvalid);
        }

        var result = await mediator.Send(
            new ImportWorkflowDefinitionFromExportCommand(
                req.Format,
                req.Version,
                req.Definition.Name,
                req.Definition.Description ?? string.Empty,
                req.Definition.Category ?? string.Empty,
                req.Definition.DesignerSchemaJson ?? string.Empty,
                userIdValue,
                req.UpsertByName),
            ct);

        await Send.OkAsync(
            new ImportWorkflowDefinitionResponse(
                result.Id,
                result.Name,
                result.Action,
                result.RemapReport,
                result.Warnings).AsResponseData(),
            cancellation: ct);
    }
}
