using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 获取用户信息的请求模型
/// </summary>
/// <param name="Id">要查询的用户ID</param>
public record GetUserRequest(UserId Id);

/// <summary>
/// 获取用户信息的API端点
/// 该端点用于根据用户ID查询用户的详细信息
/// </summary>
[Tags("Users")]
public class GetUserEndpoint(UserQuery userQuery) : Endpoint<GetUserRequest, ResponseData<UserInfoQueryDto?>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，通过路由参数获取用户ID
        Get("/api/users/{id}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和用户查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 根据用户ID查询用户详细信息并返回结果
    /// </summary>
    /// <param name="req">包含用户ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        // 通过查询服务获取用户详细信息
        var userInfo = await userQuery.GetUserByIdAsync(req.Id, ct);

        // 验证用户是否存在
        if (userInfo == null)
        {
            throw new KnownException($"未找到用户，Id = {req.Id}");
        }

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(userInfo.AsResponseData(), cancellation: ct);
    }
}

