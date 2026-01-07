using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.UserCommands;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 更新用户角色的请求模型
/// </summary>
/// <param name="UserId">要更新角色的用户ID</param>
/// <param name="RoleIds">要分配给用户的角色ID列表</param>
public record UpdateUserRolesRequest(UserId UserId, IEnumerable<RoleId> RoleIds);

/// <summary>
/// 更新用户角色的响应模型
/// </summary>
/// <param name="UserId">已更新角色的用户ID</param>
public record UpdateUserRolesResponse(UserId UserId);

/// <summary>
/// 更新用户角色的API端点
/// 该端点用于修改指定用户的角色分配，支持批量角色分配
/// </summary>
[Tags("Users")]
public class UpdateUserRolesEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<UpdateUserRolesRequest, ResponseData<UpdateUserRolesResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新用户角色分配
        Put("/api/users/update-roles");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和用户角色分配权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserRoleAssign);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 验证角色信息，更新用户角色分配并返回结果
    /// </summary>
    /// <param name="request">包含用户ID和角色ID列表的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateUserRolesRequest request, CancellationToken ct)
    {
        // 通过角色查询服务验证要分配的角色信息
        // 确保角色存在且可用于分配
        var rolesToBeAssigned = await roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);

        // 创建更新用户角色命令对象
        var cmd = new UpdateUserRolesCommand(request.UserId, rolesToBeAssigned);

        // 通过中介者发送命令，执行实际的角色分配业务逻辑
        await mediator.Send(cmd, ct);

        // 创建响应对象，包含已更新角色的用户ID
        var response = new UpdateUserRolesResponse(request.UserId);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

