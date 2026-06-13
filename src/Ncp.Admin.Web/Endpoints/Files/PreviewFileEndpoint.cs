using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Files;

/// <summary>
/// 在线预览：按存储 path 返回文件流（docx/pdf/图片等）。
/// </summary>
public class PreviewFileEndpoint(IFileStorageService fileStorage) : Endpoint<DownloadFileRequest>
{
    public override void Configure()
    {
        Tags("File");
        Description(b => b.AutoTagOverride("File").WithSummary("文件在线预览"));
        Get("/api/admin/files/preview");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess);
    }

    public override async Task HandleAsync(DownloadFileRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Path))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var path = Uri.UnescapeDataString(req.Path!);
        if (string.IsNullOrWhiteSpace(path) ||
            path.Contains("..", StringComparison.Ordinal) ||
            path.TrimStart().StartsWith("/", StringComparison.Ordinal))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var stream = await fileStorage.DownloadAsync(path, ct);
        if (stream == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var fileName = System.IO.Path.GetFileName(path);
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        if (ext == ".doc")
        {
            await stream.DisposeAsync();
            ThrowError("旧版 .doc 文件无法在线预览，请重新「生成合同文件」或上传 docx 格式。");
            return;
        }

        var contentType = DownloadFileEndpoint.GetContentTypeForPreview(fileName);
        await Send.StreamAsync(stream, fileName, stream.Length, contentType, cancellation: ct);
    }
}
