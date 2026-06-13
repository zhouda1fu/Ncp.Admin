using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Extensions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 当前登录用户上传头像请求。
/// </summary>
public class UploadCurrentUserAvatarRequest
{
    /// <summary>
    /// 头像图片文件。
    /// </summary>
    public IFormFile? File { get; set; }
}

/// <summary>
/// 当前登录用户上传并保存自己的头像。
/// </summary>
public class UploadCurrentUserAvatarEndpoint(
    IFileStorageService fileStorage,
    IMediator mediator)
    : Endpoint<UploadCurrentUserAvatarRequest, ResponseData<UploadCurrentUserAvatarResponse>>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp"
    };

    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("当前用户上传头像"));
        Post("/api/admin/user/avatar/upload");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadCurrentUserAvatarRequest req, CancellationToken ct)
    {
        var currentUserId = User.GetUserIdOrNull() ?? throw new KnownException("无效的用户身份", ErrorCodes.InvalidUser);
        if (req.File == null || req.File.Length == 0)
        {
            throw new KnownException("头像文件不能为空");
        }

        if (!AllowedContentTypes.Contains(req.File.ContentType))
        {
            throw new KnownException("请选择 JPG、PNG、GIF、WEBP 或 BMP 格式的图片");
        }

        await using var stream = req.File.OpenReadStream();
        var avatarUrl = await fileStorage.UploadAsync(stream, req.File.FileName ?? "avatar", ct);
        await mediator.Send(new UpdateCurrentUserAvatarCommand(currentUserId, avatarUrl), ct);
        await Send.OkAsync(new UploadCurrentUserAvatarResponse(avatarUrl).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 当前用户头像上传响应。
/// </summary>
/// <param name="AvatarUrl">头像存储地址</param>
public record UploadCurrentUserAvatarResponse(string AvatarUrl);
