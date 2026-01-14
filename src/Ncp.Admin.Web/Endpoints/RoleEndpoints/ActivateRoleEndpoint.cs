using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.RoleCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 激活角色的请求模型
/// </summary>
/// <param name="RoleId">要激活的角色ID</param>
public record ActivateRoleRequest(RoleId RoleId);

/// <summary>
/// 激活角色的API端点
/// 该端点用于激活已停用的角色
/// </summary>
[Tags("Roles")]
public class ActivateRoleEndpoint(IMediator mediator) : Endpoint<ActivateRoleRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于激活角色
        Put("/api/roles/activate");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和角色编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回激活结果
    /// </summary>
    /// <param name="req">包含角色ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(ActivateRoleRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        var cmd = new ActivateRoleCommand(req.RoleId);

        // 通过中介者发送命令，执行实际的业务逻辑
        await mediator.Send(cmd, ct);

        // 返回成功响应，表示激活操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
