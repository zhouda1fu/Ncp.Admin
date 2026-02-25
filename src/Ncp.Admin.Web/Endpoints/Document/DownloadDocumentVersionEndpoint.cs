using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Document;

/// <summary>
/// 下载文档指定版本
/// </summary>
public class DownloadDocumentVersionEndpoint(DocumentQuery documentQuery, IFileStorageService fileStorage)
    : Endpoint<DownloadDocumentVersionRequest>
{
    public override void Configure()
    {
        Tags("Document");
        Get("/api/admin/documents/{documentId}/versions/{versionId}/download");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentView);
    }

    public override async Task HandleAsync(DownloadDocumentVersionRequest req, CancellationToken ct)
    {
        var info = await documentQuery.GetVersionDownloadInfoAsync(new DocumentVersionId(req.VersionId), ct);
        if (info == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var stream = await fileStorage.DownloadAsync(info.Value.FileStorageKey, ct);
        if (stream == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.StreamAsync(stream, info.Value.FileName, stream.Length, "application/octet-stream", cancellation: ct);
    }
}

public class DownloadDocumentVersionRequest
{
    public Guid DocumentId { get; set; }
    public Guid VersionId { get; set; }
}
