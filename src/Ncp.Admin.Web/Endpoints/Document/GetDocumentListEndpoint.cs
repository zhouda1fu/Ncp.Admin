using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Document;

/// <summary>
/// 获取文档列表请求
/// </summary>
public class GetDocumentListRequest : DocumentQueryInput { }

/// <summary>
/// 获取文档分页列表
/// </summary>
public class GetDocumentListEndpoint(DocumentQuery query)
    : Endpoint<GetDocumentListRequest, ResponseData<PagedData<DocumentQueryDto>>>
{
    public override void Configure()
    {
        Tags("Document");
        Get("/api/admin/documents");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentView);
    }

    public override async Task HandleAsync(GetDocumentListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
