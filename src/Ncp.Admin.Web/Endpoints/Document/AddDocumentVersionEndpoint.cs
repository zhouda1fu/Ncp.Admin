using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Commands.Document;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Document;

/// <summary>
/// 上传新版本请求（multipart: file）
/// </summary>
public class AddDocumentVersionRequest
{
    public Guid DocumentId { get; set; }
    public IFormFile? File { get; set; }
}

/// <summary>
/// 为文档添加新版本（上传新文件）
/// </summary>
public class AddDocumentVersionEndpoint(IMediator mediator, IFileStorageService fileStorage)
    : Endpoint<AddDocumentVersionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Document");
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
            new DocumentId(req.DocumentId), fileKey, req.File.FileName ?? "file", req.File.Length);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
