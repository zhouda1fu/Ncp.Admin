using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.Definition;

public record ExportWorkflowDefinitionRequest(WorkflowDefinitionId Id);

/// <summary>
/// 导出流程定义 JSON（含身份名称目录，便于库重置后导入重映射）。
/// GET /api/admin/workflow/definitions/{id}/export
/// </summary>
public class ExportWorkflowDefinitionEndpoint(IMediator mediator)
    : Endpoint<ExportWorkflowDefinitionRequest, ResponseData<WorkflowDefinitionExportDocument>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions").WithSummary("导出流程定义 JSON（含身份目录）"));
        Get("/api/admin/workflow/definitions/{id}/export");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionView);
    }

    public override async Task HandleAsync(ExportWorkflowDefinitionRequest req, CancellationToken ct)
    {
        var doc = await mediator.Send(new ExportWorkflowDefinitionQuery(req.Id), ct)
            ?? throw new KnownException("未找到流程定义", ErrorCodes.WorkflowDefinitionNotFound);

        await Send.OkAsync(doc.AsResponseData(), cancellation: ct);
    }
}
