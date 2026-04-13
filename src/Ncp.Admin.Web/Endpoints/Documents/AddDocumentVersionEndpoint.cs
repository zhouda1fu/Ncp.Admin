using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Commands.Documents;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Documents;

/// <summary>
/// 上传新版本请求（multipart: file）
/// </summary>
/// <param name="DocumentId">文档 ID</param>
/// <param name="File">上传文件</param>
public record AddDocumentVersionRequest(DocumentId DocumentId, IFormFile? File);

/// <summary>
/// 为文档添加新版本（上传新文件）
/// </summary>
public class AddDocumentVersionEndpoint(IMediator mediator, IFileStorageService fileStorage)
    : Endpoint<AddDocumentVersionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Document");
        Description(b => b.AutoTagOverride("Document").WithSummary("为文档添加新版本（上传新文件）"));
        Post("/api/admin/documents/{documentId}/versions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentEdit);
        AllowFileUploads();
    }

    public override async Task HandleAsync(AddDocumentVersionRequest req, CancellationToken ct)
    {
        if (req.File == null || req.File.Length == 0)
        {
            AddError("file", "请选择文件");
            ThrowIfAnyErrors();
            return;
        }
        await using var stream = req.File.OpenReadStream();
        var fileKey = await fileStorage.UploadAsync(stream, req.File.FileName ?? "file", ct);
        var cmd = new AddDocumentVersionCommand(
            req.DocumentId, fileKey, req.File.FileName ?? "file", req.File.Length);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
