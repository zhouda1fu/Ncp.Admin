using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.UserCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 删除用户的请求模型
/// </summary>
/// <param name="UserId">要删除的用户ID</param>
public record DeleteUserRequest(UserId UserId);

/// <summary>
/// 删除用户的API端点
/// 该端点用于从系统中删除指定的用户账户（软删除）
/// </summary>
[Tags("Users")]
public class DeleteUserEndpoint(IMediator mediator) : Endpoint<DeleteUserRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP DELETE方法，用于删除用户
        Delete("/api/users/{userId}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和用户删除权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserDelete);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 从路由获取用户ID，执行删除操作并返回结果
    /// </summary>
    /// <param name="request">包含要删除的用户ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        // 创建删除用户命令对象
        var command = new DeleteUserCommand(request.UserId);

        // 通过中介者发送命令，执行实际的删除业务逻辑（软删除）
        await mediator.Send(command, ct);

        // 返回成功响应，表示删除操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

