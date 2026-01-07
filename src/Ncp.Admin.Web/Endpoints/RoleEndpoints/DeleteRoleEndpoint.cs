using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.RoleCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 删除角色的请求模型
/// </summary>
/// <param name="RoleId">要删除的角色ID</param>
public record DeleteRoleRequest(RoleId RoleId);

/// <summary>
/// 删除角色的API端点
/// 该端点用于从系统中删除指定的角色（软删除）
/// </summary>
[Tags("Roles")]
public class DeleteRoleEndpoint(IMediator mediator) : Endpoint<DeleteRoleRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP DELETE方法，用于删除角色
        Delete("/api/roles/{roleId}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和角色删除权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleDelete);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 从路由获取角色ID，执行删除操作并返回结果
    /// </summary>
    /// <param name="request">包含要删除的角色ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(DeleteRoleRequest request, CancellationToken ct)
    {
        // 创建删除角色命令对象
        var command = new DeleteRoleCommand(request.RoleId);

        // 通过中介者发送命令，执行实际的删除业务逻辑（软删除）
        await mediator.Send(command, ct);

        // 返回成功响应，表示删除操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

