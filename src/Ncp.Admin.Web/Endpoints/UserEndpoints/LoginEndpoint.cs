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

public record LoginResponse(string Token, string RefreshToken, UserId UserId, string Name, string Email, string Permissions, DateTimeOffset TokenExpiryTime);

[Tags("Users")]
[HttpPost("/api/user/login")]
[AllowAnonymous]
public class LoginEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery) : Endpoint<LoginRequest, ResponseData<LoginResponse>>
{
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var loginInfo = await userQuery.GetUserInfoForLoginAsync(req.Username, ct) ?? throw new KnownException("无效的用户");

        if (!PasswordHasher.VerifyHashedPassword(req.Password, loginInfo.PasswordHash))
            throw new KnownException("用户名或密码错误");

        var refreshToken = TokenGenerator.GenerateRefreshToken();
        var nowTime = DateTimeOffset.Now;
        var tokenExpiryTime = nowTime.AddMinutes(appConfiguration.Value.TokenExpiryInMinutes);

        var roles = loginInfo.UserRoles.Select(r => r.RoleId).ToList();

        var assignedPermissionCode = await roleQuery.GetAssignedPermissionCodesAsync(roles, ct);

        var claims = new List<Claim>
        {
            new("name", loginInfo.Name),
            new("email", loginInfo.Email),
            new("sub", loginInfo.UserId.ToString()),
            new("user_id", loginInfo.UserId.ToString())
        };

        if (assignedPermissionCode != null)
        {
            foreach (var permissionCode in assignedPermissionCode)
            {
                claims.Add(new Claim("permissions", permissionCode));
            }
        }

        var token = await jwtProvider.GenerateJwtToken(new JwtData("issuer-x", "audience-y", claims, nowTime.UtcDateTime, tokenExpiryTime.UtcDateTime), ct);

        var response = new LoginResponse(
            token,
            refreshToken,
            loginInfo.UserId,
            loginInfo.Name,
            loginInfo.Email,
            JsonSerializer.Serialize(assignedPermissionCode),
            tokenExpiryTime
        );

        var updateCmd = new UpdateUserLoginTimeCommand(loginInfo.UserId, DateTimeOffset.UtcNow, refreshToken);
        await mediator.Send(updateCmd, ct);

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}