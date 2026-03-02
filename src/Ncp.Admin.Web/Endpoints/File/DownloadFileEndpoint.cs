using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.File;

/// <summary>
/// 按存储 path 下载文件（path 为上传接口返回的相对路径，需 URL 编码）。
/// </summary>
public class DownloadFileEndpoint(IFileStorageService fileStorage) : Endpoint<DownloadFileRequest>
{
    public override void Configure()
    {
        Tags("File");
        Get("/api/admin/files/download");
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
        var contentType = GetContentType(fileName);
        await Send.StreamAsync(stream, fileName, stream.Length, contentType, cancellation: ct);
    }

    private static string GetContentType(string fileName)
    {
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}

public class DownloadFileRequest
{
    public string? Path { get; set; }
}
