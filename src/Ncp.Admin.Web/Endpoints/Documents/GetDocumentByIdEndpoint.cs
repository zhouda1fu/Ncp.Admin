using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Documents;

/// <summary>
/// 按 ID 获取文档详情
/// </summary>
public class GetDocumentByIdEndpoint(DocumentQuery query)
    : Endpoint<GetDocumentByIdRequest, ResponseData<DocumentQueryDto>>
{
    public override void Configure()
    {
        Tags("Document");
        Description(b => b.AutoTagOverride("Document").WithSummary("按 ID 获取文档详情"));
        Get("/api/admin/documents/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentView);
    }

    public override async Task HandleAsync(GetDocumentByIdRequest req, CancellationToken ct)
    {
        var doc = await query.GetByIdAsync(req.Id, ct);
        if (doc == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(doc.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 按 ID 获取文档请求
/// </summary>
/// <param name="Id">文档 ID</param>
public record GetDocumentByIdRequest(DocumentId Id);
