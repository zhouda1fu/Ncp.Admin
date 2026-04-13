using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Files;

/// <summary>
/// 通用文件上传请求（multipart: file）
/// </summary>
public class UploadFileRequest
{
    public IFormFile? File { get; set; }
}

/// <summary>
/// 通用文件上传，仅存储并返回 path，不落业务表。
/// </summary>
public class UploadFileEndpoint(IFileStorageService fileStorage)
    : Endpoint<UploadFileRequest, ResponseData<UploadFileResponse>>
{
    public override void Configure()
    {
        Tags("File");
        Description(b => b.AutoTagOverride("File").WithSummary("通用文件上传，仅存储并返回 path，不落业务表。"));
        Post("/api/admin/files/upload");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadFileRequest req, CancellationToken ct)
    {
        if (req.File == null || req.File.Length == 0)
        {
            AddError("request", "文件不能为空");
            ThrowIfAnyErrors();
            return;
        }
        await using var stream = req.File.OpenReadStream();
        var path = await fileStorage.UploadAsync(stream, req.File.FileName ?? "file", ct);
        await Send.OkAsync(new UploadFileResponse(path).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 上传返回的存储路径，用于后续下载或存入业务字段（如 Customer.BusinessLicense）。
/// </summary>
public record UploadFileResponse(string Path);
