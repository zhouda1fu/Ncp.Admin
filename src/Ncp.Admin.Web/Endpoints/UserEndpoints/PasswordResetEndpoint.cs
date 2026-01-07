using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.UserCommands;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 密码重置的请求模型
/// </summary>
/// <param name="UserId">需要重置密码的用户ID</param>
public record PasswordResetRequest(UserId UserId);

/// <summary>
/// 密码重置的响应模型
/// </summary>
/// <param name="UserId">已重置密码的用户ID</param>
public record PasswordResetResponse(UserId UserId);

/// <summary>
/// 密码重置的API端点
/// 该端点用于重置指定用户的密码为默认密码（123456）
/// </summary>
[Tags("Users")]
public class PasswordResetEndpoint(IMediator mediator) : Endpoint<PasswordResetRequest, ResponseData<PasswordResetResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新用户密码
        Put("/api/user/password-reset");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和用户编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将用户密码重置为默认密码（123456）并返回结果
    /// </summary>
    /// <param name="request">包含用户ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(PasswordResetRequest request, CancellationToken ct)
    {
        // 对默认密码进行哈希处理
        // 注意：这里硬编码了默认密码"123456"，在生产环境中应该考虑更安全的密码策略
        var passwordHash = PasswordHasher.HashPassword("123456");

        // 创建密码重置命令对象
        var cmd = new PasswordResetCommand(request.UserId, passwordHash);

        // 通过中介者发送命令，执行实际的密码重置业务逻辑
        // 返回已重置密码的用户ID
        var userId = await mediator.Send(cmd, ct);

        // 创建响应对象，包含已重置密码的用户ID
        var response = new PasswordResetResponse(userId);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

