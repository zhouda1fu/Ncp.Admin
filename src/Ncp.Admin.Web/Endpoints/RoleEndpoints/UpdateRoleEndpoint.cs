using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.RoleCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 更新角色信息的请求模型
/// </summary>
/// <param name="RoleId">要更新的角色ID</param>
/// <param name="Name">新的角色名称</param>
/// <param name="Description">新的角色描述</param>
/// <param name="PermissionCodes">新的权限代码列表</param>
public record UpdateRoleInfoRequest(RoleId RoleId, string Name, string Description, IEnumerable<string> PermissionCodes);

/// <summary>
/// 更新角色信息的API端点
/// 该端点用于修改现有角色的基本信息和权限分配
/// </summary>
[Tags("Roles")]
public class UpdateRoleEndpoint(IMediator mediator) : Endpoint<UpdateRoleInfoRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新角色信息
        Put("/api/roles/update");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和角色编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回更新结果
    /// </summary>
    /// <param name="request">包含角色更新信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateRoleInfoRequest request, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        var cmd = new UpdateRoleInfoCommand(
            request.RoleId,           // 要更新的角色ID
            request.Name,             // 新的角色名称
            request.Description,      // 新的角色描述
            request.PermissionCodes   // 新的权限代码列表
        );

        // 通过中介者发送命令，执行实际的更新业务逻辑
        await mediator.Send(cmd, ct);

        // 返回成功响应，表示更新操作完成
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

