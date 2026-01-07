using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 获取角色信息的请求模型
/// </summary>
/// <param name="Id">要查询的角色ID</param>
public record GetRoleRequest(RoleId Id);

/// <summary>
/// 获取角色信息的API端点
/// 该端点用于根据角色ID查询角色的详细信息，包括权限列表
/// </summary>
[Tags("Roles")]
public class GetRoleEndpoint(RoleQuery roleQuery) : Endpoint<GetRoleRequest, ResponseData<RoleQueryDto?>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，通过路由参数获取角色ID
        Get("/api/roles/{id}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和角色查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 根据角色ID查询角色详细信息并返回结果
    /// </summary>
    /// <param name="req">包含角色ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetRoleRequest req, CancellationToken ct)
    {
        // 通过查询服务获取角色详细信息
        var roleInfo = await roleQuery.GetRoleByIdAsync(req.Id, ct);

        // 验证角色是否存在
        if (roleInfo == null)
        {
            throw new KnownException($"未找到角色，Id = {req.Id}");
        }

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(roleInfo!.AsResponseData(), cancellation: ct);
    }
}

