using System.Security.Claims;
using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Utils;
using Serilog;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 登录的请求模型
/// </summary>
/// <param name="Username">用户名</param>
/// <param name="Password">密码</param>
public record LoginRequest(string Username, string Password);

/// <summary>
/// 登录的响应模型
/// </summary>
/// <param name="Token">JWT访问令牌</param>
/// <param name="RefreshToken">刷新令牌</param>
/// <param name="UserId">用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Roles">角色列表（JSON字符串）</param>
/// <param name="PermissionCodes">权限代码列表</param>
/// <param name="TokenExpiryTime">令牌过期时间</param>
public record LoginResponse(string Token, string RefreshToken, UserId UserId, string Name, string Email, string Roles, IEnumerable<string> PermissionCodes, DateTimeOffset TokenExpiryTime);

/// <summary>
/// 登录
/// </summary>
/// <param name="mediator"></param>
/// <param name="userQuery"></param>
/// <param name="jwtProvider"></param>
/// <param name="appConfiguration"></param>
/// <param name="roleQuery"></param>
public class LoginEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery) : Endpoint<LoginRequest, ResponseData<LoginResponse>>
{
    private const string PermissionClaimType = "permissions";

    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Post("/api/admin/user/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        Log.Information("用户登录尝试: Username={Username}", req.Username);
        
        var loginInfo = await userQuery.GetUserInfoForLoginAsync(req.Username, ct);
        
        if (loginInfo == null || !PasswordHasher.VerifyHashedPassword(req.Password, loginInfo.PasswordHash))
        {
            throw new KnownException("用户名或密码错误", ErrorCodes.UserNameOrPasswordError);
        }

        var nowTime = DateTimeOffset.UtcNow;
        var tokenExpiryTime = nowTime.AddMinutes(appConfiguration.Value.TokenExpiryInMinutes);
        var refreshToken = TokenGenerator.GenerateRefreshToken();
        var roles = loginInfo.UserRoles.Select(r => r.RoleId).ToList();
        var assignedPermissionCodes = roles.Count > 0
            ? await roleQuery.GetAssignedPermissionCodesAsync(roles, ct)
            : Enumerable.Empty<string>();
        var claims = BuildClaims(loginInfo, assignedPermissionCodes);
        var config = appConfiguration.Value;
        var token = await jwtProvider.GenerateJwtToken(
            new JwtData(
                config.JwtIssuer,
                config.JwtAudience,
                claims,
                nowTime.UtcDateTime,
                tokenExpiryTime.UtcDateTime),
            ct);
        var response = new LoginResponse(
            token,
            refreshToken,
            loginInfo.UserId,
            loginInfo.Name,
            loginInfo.Email,
            JsonSerializer.Serialize(roles) ?? "[]",
            assignedPermissionCodes,
            tokenExpiryTime
        );

        // 更新用户登录时间和刷新令牌
        var updateCmd = new UpdateUserLoginTimeCommand(loginInfo.UserId, nowTime, refreshToken);
        await mediator.Send(updateCmd, ct);

        Log.Information("用户登录成功: UserId={UserId}, Username={Username}, Email={Email}, RoleCount={RoleCount}, PermissionCount={PermissionCount}", 
            loginInfo.UserId, loginInfo.Name, loginInfo.Email, roles.Count, assignedPermissionCodes.Count());

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