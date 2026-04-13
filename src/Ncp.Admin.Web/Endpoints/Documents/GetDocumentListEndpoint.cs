using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Documents;

/// <summary>
/// 获取文档列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Title">标题关键字</param>
public record GetDocumentListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Title = null);

/// <summary>
/// 获取文档分页列表
/// </summary>
public class GetDocumentListEndpoint(DocumentQuery query)
    : Endpoint<GetDocumentListRequest, ResponseData<PagedData<DocumentQueryDto>>>
{
    public override void Configure()
    {
        Tags("Document");
        Description(b => b.AutoTagOverride("Document").WithSummary("获取文档分页列表"));
        Get("/api/admin/documents");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentView);
    }

    public override async Task HandleAsync(GetDocumentListRequest req, CancellationToken ct)
    {
        var input = new DocumentQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Title = req.Title,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
