using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.Dto;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Services;
using Serilog;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 退出登录
/// </summary>
/// <param name="mediator"></param>
public class LogoutEndpoint(
    IMediator mediator,
    IUserSessionService userSessionService,
    IDistributedLock distributedLock) : EndpointWithoutRequest<ResponseData<bool>>
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
        var sessionId = User.FindFirst(UserSessionClaimTypes.SessionId)?.Value;
        if (User.TryGetUserId(out var userId) && !string.IsNullOrWhiteSpace(sessionId))
        {
            try
            {
                await using var sessionLock = await distributedLock.AcquireAsync(
                    $"auth:session-lock:{userId.Id}",
                    cancellationToken: ct);

                if (await userSessionService.RemoveIfCurrentAsync(userId.Id, sessionId))
                {
                    var revokeCmd = new RevokeUserRefreshTokensCommand(userId);
                    await mediator.Send(revokeCmd, ct);
                }
            }
            catch (Exception ex)
            {
                // 即使撤销 token 失败，也记录日志但不影响退出流程
                Log.Warning(ex, "用户退出登录时撤销刷新令牌失败，用户ID：{UserId}", userId);
            }
        }
        
        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
