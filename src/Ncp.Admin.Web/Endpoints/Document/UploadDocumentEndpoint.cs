using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Commands.Document;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Document;

/// <summary>
/// 上传文档请求（multipart: title + file）
/// </summary>
public class UploadDocumentRequest
{
    public string Title { get; set; } = "";
    public IFormFile? File { get; set; }
}

/// <summary>
/// 上传文档（创建文档+首版）
/// </summary>
public class UploadDocumentEndpoint(IMediator mediator, IFileStorageService fileStorage)
    : Endpoint<UploadDocumentRequest, ResponseData<UploadDocumentResponse>>
{
    public override void Configure()
    {
        Tags("Document");
        Post("/api/admin/documents/upload");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentCreate);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadDocumentRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || req.File == null || req.File.Length == 0)
        {
            AddError("request", "标题和文件不能为空");
            ThrowIfAnyErrors();
            return;
        }
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        await using var stream = req.File.OpenReadStream();
        var fileKey = await fileStorage.UploadAsync(stream, req.File.FileName ?? "file", ct);
        var cmd = new CreateDocumentCommand(
            new UserId(uid), req.Title.Trim(), fileKey, req.File.FileName ?? "file", req.File.Length);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new UploadDocumentResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record UploadDocumentResponse(DocumentId Id);
