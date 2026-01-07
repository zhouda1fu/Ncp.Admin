using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.RoleCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 创建角色的请求模型
/// </summary>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
/// <param name="PermissionCodes">权限代码列表</param>
public record CreateRoleRequest(string Name, string Description, IEnumerable<string> PermissionCodes);

/// <summary>
/// 创建角色的响应模型
/// </summary>
/// <param name="RoleId">新创建的角色ID</param>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
public record CreateRoleResponse(RoleId RoleId, string Name, string Description);

/// <summary>
/// 创建角色的API端点
/// 该端点用于在系统中创建新的角色，并分配相应的权限
/// </summary>
[Tags("Roles")]
public class CreateRoleEndpoint(IMediator mediator) : Endpoint<CreateRoleRequest, ResponseData<CreateRoleResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于创建新的角色
        Post("/api/roles");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和角色创建权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleCreate);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回新创建的角色信息
    /// </summary>
    /// <param name="req">包含角色基本信息和权限的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        var cmd = new CreateRoleCommand(req.Name, req.Description, req.PermissionCodes);

        // 通过中介者发送命令，执行实际的业务逻辑
        // 返回新创建的角色ID
        var result = await mediator.Send(cmd, ct);

        // 创建响应对象，包含新创建的角色信息
        var response = new CreateRoleResponse(
            result,        // 新创建的角色ID
            req.Name,      // 角色名称
            req.Description // 角色描述
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

