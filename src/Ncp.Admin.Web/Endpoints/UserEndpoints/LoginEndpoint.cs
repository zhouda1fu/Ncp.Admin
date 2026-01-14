using System.Security.Claims;
using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.UserCommands;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string RefreshToken, UserId UserId, string Name, string Email, string Roles, IEnumerable<string> PermissionCodes, DateTimeOffset TokenExpiryTime);

[Tags("Users")]
[HttpPost("/api/user/login")]
[AllowAnonymous]
public class LoginEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery) : Endpoint<LoginRequest, ResponseData<LoginResponse>>
{
    private const string PermissionClaimType = "permissions";

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // 查询用户信息
        var loginInfo = await userQuery.GetUserInfoForLoginAsync(req.Username, ct);
        
        // 统一错误消息，防止时序攻击（通过响应时间推断用户是否存在）
        if (loginInfo == null || !PasswordHasher.VerifyHashedPassword(req.Password, loginInfo.PasswordHash))
        {
            throw new KnownException("用户名或密码错误");
        }

        // 统一使用UTC时间
        var nowTime = DateTimeOffset.UtcNow;
        var tokenExpiryTime = nowTime.AddMinutes(appConfiguration.Value.TokenExpiryInMinutes);
        var refreshToken = TokenGenerator.GenerateRefreshToken();

        // 获取用户角色ID列表
        var roles = loginInfo.UserRoles.Select(r => r.RoleId).ToList();

        // 查询权限代码（如果用户没有角色，则跳过查询）
        var assignedPermissionCodes = roles.Count > 0
            ? await roleQuery.GetAssignedPermissionCodesAsync(roles, ct)
            : Enumerable.Empty<string>();

        // 构建JWT Claims（包含权限代码）
        var claims = BuildClaims(loginInfo, assignedPermissionCodes);

        // 生成JWT Token
        var config = appConfiguration.Value;
        var token = await jwtProvider.GenerateJwtToken(
            new JwtData(
                config.JwtIssuer,
                config.JwtAudience,
                claims,
                nowTime.UtcDateTime,
                tokenExpiryTime.UtcDateTime),
            ct);

        // 构建响应
        var response = new LoginResponse(
            token,
            refreshToken,
            loginInfo.UserId,
            loginInfo.Name,
            loginInfo.Email,
            JsonSerializer.Serialize(roles) ?? "[]",
            assignedPermissionCodes, // 直接返回权限代码列表
            tokenExpiryTime
        );

        // 更新用户登录时间和刷新令牌
        var updateCmd = new UpdateUserLoginTimeCommand(loginInfo.UserId, nowTime, refreshToken);
        await mediator.Send(updateCmd, ct);

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }

    /// <summary>
    /// 构建JWT Claims
    /// </summary>
    private static List<Claim> BuildClaims(UserLoginInfoQueryDto loginInfo, IEnumerable<string> permissionCodes)
    {
        var userIdString = loginInfo.UserId.ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, loginInfo.Name),
            new(ClaimTypes.Email, loginInfo.Email),
            new(ClaimTypes.NameIdentifier, userIdString)
        };
       
        claims.AddRange(permissionCodes.Select(code => new Claim(PermissionClaimType, code)));

        return claims;
    }
}