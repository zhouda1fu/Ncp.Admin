using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Document;

/// <summary>
/// 按 ID 获取文档详情
/// </summary>
public class GetDocumentByIdEndpoint(DocumentQuery query)
    : Endpoint<GetDocumentByIdRequest, ResponseData<DocumentQueryDto>>
{
    public override void Configure()
    {
        Tags("Document");
        Get("/api/admin/documents/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentView);
    }

    public override async Task HandleAsync(GetDocumentByIdRequest req, CancellationToken ct)
    {
        var doc = await query.GetByIdAsync(new DocumentId(req.Id), ct);
        if (doc == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(doc.AsResponseData(), cancellation: ct);
    }
}

public class GetDocumentByIdRequest
{
    public Guid Id { get; set; }
}
