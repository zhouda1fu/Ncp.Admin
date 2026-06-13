using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Extensions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 当前登录用户更新头像请求。
/// </summary>
/// <param name="AvatarUrl">头像文件地址</param>
public record UpdateCurrentUserAvatarRequest(string AvatarUrl);

/// <summary>
/// 当前登录用户更新自己的头像。
/// </summary>
public class UpdateCurrentUserAvatarEndpoint(IMediator mediator)
    : Endpoint<UpdateCurrentUserAvatarRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("当前用户更新头像"));
        Put("/api/admin/user/avatar");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateCurrentUserAvatarRequest req, CancellationToken ct)
    {
        var currentUserId = User.GetUserIdOrNull() ?? throw new KnownException("无效的用户身份", ErrorCodes.InvalidUser);
        var result = await mediator.Send(new UpdateCurrentUserAvatarCommand(currentUserId, req.AvatarUrl), ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
