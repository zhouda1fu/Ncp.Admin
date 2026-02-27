using System.Security.Claims;
using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Services;
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
/// <param name="DataScope">数据权限范围（与角色一致，取最宽松）</param>
/// <param name="DeptId">所属部门 ID，无部门时为 null</param>
/// <param name="TokenExpiryTime">令牌过期时间</param>
public record LoginResponse(string Token, string RefreshToken, UserId UserId, string Name, string Email, string Roles, IEnumerable<string> PermissionCodes, DataScope DataScope, DeptId? DeptId, DateTimeOffset TokenExpiryTime);

/// <summary>
/// 登录
/// </summary>
/// <param name="mediator"></param>
/// <param name="userQuery"></param>
/// <param name="jwtProvider"></param>
/// <param name="appConfiguration"></param>
/// <param name="roleQuery"></param>
/// <param name="deptQuery">用于登录时计算 authorized_dept_ids（本部门及子部门）</param>
/// <param name="passwordHasher"></param>
/// <param name="refreshTokenGenerator"></param>
public class LoginEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery, DeptQuery deptQuery, IPasswordHasher passwordHasher, IRefreshTokenGenerator refreshTokenGenerator) : Endpoint<LoginRequest, ResponseData<LoginResponse>>
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
        
        if (loginInfo == null || !passwordHasher.Verify(req.Password, loginInfo.PasswordHash))
        {
            throw new KnownException("用户名或密码错误", ErrorCodes.UserNameOrPasswordError);
        }

        var nowTime = DateTimeOffset.UtcNow;
        var tokenExpiryTime = nowTime.AddMinutes(appConfiguration.Value.TokenExpiryInMinutes);
        var refreshToken = refreshTokenGenerator.Generate();
        var roles = loginInfo.UserRoles.Select(r => r.RoleId).ToList();
        var adminRoles = roles.Count > 0
            ? await roleQuery.GetAdminRolesForAssignmentAsync(roles, ct)
            : [];
        var assignedPermissionCodes = adminRoles.SelectMany(r => r.PermissionCodes).Distinct();
        var dataScope = adminRoles.Count > 0
            ? (DataScope)adminRoles.Min(r => (int)r.DataScope)
            : DataScope.All;
        var userInfo = await userQuery.GetUserByIdAsync(loginInfo.UserId, ct);
        var deptId = userInfo?.DeptId;
        var authorizedDeptIds = await GetAuthorizedDeptIdsAsync(dataScope, deptId, ct);
        var claims = BuildClaims(loginInfo, assignedPermissionCodes, dataScope, deptId, authorizedDeptIds);
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
            dataScope,
            deptId,
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
    /// 登录时计算有权访问的部门 ID 列表（DeptAndSub 含本部门及子部门，供 JWT 写入 authorized_dept_ids）
    /// </summary>
    private async Task<IReadOnlyList<DeptId>> GetAuthorizedDeptIdsAsync(DataScope dataScope, DeptId? deptId, CancellationToken ct)
    {
        if (deptId == null) return [];
        if (dataScope == DataScope.DeptAndSub)
            return await deptQuery.GetAllChildDeptIdsAsync(deptId, ct);
        return [deptId];
    }

    /// <summary>
    /// 构建JWT Claims（含 data_scope、dept_id、authorized_dept_ids，供服务端从 JWT 解析）
    /// </summary>
    private static List<Claim> BuildClaims(UserLoginInfoQueryDto loginInfo, IEnumerable<string> permissionCodes, DataScope dataScope, DeptId? deptId, IReadOnlyList<DeptId> authorizedDeptIds)
    {
        var userIdString = loginInfo.UserId.ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, loginInfo.Name),
            new(ClaimTypes.Email, loginInfo.Email),
            new(ClaimTypes.NameIdentifier, userIdString),
            new("data_scope", ((int)dataScope).ToString()),
            new("dept_id", deptId?.Id.ToString()?? string.Empty ),
            new("authorized_dept_ids", string.Join(",", authorizedDeptIds.Select(d => d.Id)))
        };

        claims.AddRange(permissionCodes.Select(code => new Claim(PermissionClaimType, code)));

        return claims;
    }
}