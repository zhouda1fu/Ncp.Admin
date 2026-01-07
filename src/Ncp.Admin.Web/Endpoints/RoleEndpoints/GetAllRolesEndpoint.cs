using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.RoleEndpoints;

/// <summary>
/// 获取所有角色的API端点
/// 该端点用于查询系统中的所有角色信息，支持分页和筛选
/// </summary>
[Tags("Roles")]
public class GetAllRolesEndpoint(RoleQuery roleQuery) : Endpoint<RoleQueryInput, ResponseData<PagedData<RoleQueryDto>>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询角色信息
        Get("/api/roles");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和角色查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 查询所有角色信息并返回分页结果
    /// </summary>
    /// <param name="req">角色查询输入参数，包含分页和筛选条件</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(RoleQueryInput req, CancellationToken ct)
    {
        // 通过查询服务获取所有角色信息
        var roleInfo = await roleQuery.GetAllRolesAsync(req, ct);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(roleInfo.AsResponseData(), cancellation: ct);
    }
}

