using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Extensions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 当前登录用户修改自己的登录密码。
/// </summary>
/// <param name="OldPassword">当前密码</param>
/// <param name="NewPassword">新密码</param>
public record ChangeCurrentPasswordRequest(string OldPassword, string NewPassword);

/// <summary>
/// 修改当前用户密码。
/// </summary>
public class ChangeCurrentPasswordEndpoint(IMediator mediator)
    : Endpoint<ChangeCurrentPasswordRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("当前用户修改密码"));
        Put("/api/admin/user/change-password");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ChangeCurrentPasswordRequest req, CancellationToken ct)
    {
        var currentUserId = User.GetUserIdOrNull() ?? throw new KnownException("无效的用户身份", ErrorCodes.InvalidUser);
        var result = await mediator.Send(new ChangeCurrentPasswordCommand(currentUserId, req.OldPassword, req.NewPassword), ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
