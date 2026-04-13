using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Serilog;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 退出登录
/// </summary>
/// <param name="mediator"></param>
public class LogoutEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("退出登录"));
        Post("/api/admin/auth/logout");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // 尝试从 JWT 中获取用户 ID（若 token 仍有效）
        if (User.TryGetUserId(out var userId))
        {
            try
            {
                // 撤销用户的所有刷新令牌，防止 token 被继续使用
                var revokeCmd = new RevokeUserRefreshTokensCommand(userId);
                await mediator.Send(revokeCmd, ct);

                Log.Information("用户退出登录成功: UserId={UserId}", userId);
            }
            catch (Exception ex)
            {
                // 即使撤销 token 失败，也记录日志但不影响退出流程
                Log.Warning(ex, "用户退出登录时撤销刷新令牌失败: UserId={UserId}", userId);
            }
        }
        else
        {
            Log.Information("用户退出登录: Token已失效或未提供");
        }

        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
