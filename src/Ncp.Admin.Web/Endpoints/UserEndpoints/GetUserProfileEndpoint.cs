using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 获取用户资料的请求模型
/// </summary>
/// <param name="UserId">要查询的用户ID</param>
public record GetUserProfileRequest(UserId UserId);

/// <summary>
/// 用户资料的响应模型
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Phone">电话号码</param>
/// <param name="Roles">用户角色列表</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态</param>
/// <param name="Email">邮箱地址</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="BirthDate">出生日期</param>
/// <param name="OrganizationUnitId">组织单位ID（可为空）</param>
/// <param name="OrganizationUnitName">组织单位名称</param>
public record UserProfileResponse(UserId UserId, string Name, string Phone, IEnumerable<string> Roles, string RealName, int Status, string Email, DateTimeOffset CreatedAt, string Gender, int Age, DateTimeOffset BirthDate, OrganizationUnitId? OrganizationUnitId, string OrganizationUnitName);

/// <summary>
/// 获取用户资料的API端点
/// 该端点用于根据用户ID查询用户的详细资料信息，包括基本信息、角色、状态和组织单位等
/// </summary>
[Tags("Users")]
public class GetUserProfileEndpoint(UserQuery userQuery) : Endpoint<GetUserProfileRequest, ResponseData<UserProfileResponse?>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，通过路由参数获取用户ID
        Get("/api/user/profile/{userId}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和用户查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 根据用户ID查询用户详细信息，构建响应对象并返回结果
    /// </summary>
    /// <param name="req">包含用户ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetUserProfileRequest req, CancellationToken ct)
    {
        // 通过查询服务获取用户详细信息
        var userInfo = await userQuery.GetUserByIdAsync(req.UserId, ct);

        // 验证用户是否存在，如果不存在则抛出异常
        if (userInfo == null)
        {
            throw new KnownException("无效的用户");
        }

        // 创建用户资料响应对象，包含所有用户信息
        var response = new UserProfileResponse(
            userInfo.UserId,              // 用户ID
            userInfo.Name,                // 用户名
            userInfo.Phone,               // 电话号码
            userInfo.Roles,               // 用户角色列表
            userInfo.RealName,            // 真实姓名
            userInfo.Status,              // 用户状态
            userInfo.Email,               // 邮箱地址
            userInfo.CreatedAt,           // 创建时间
            userInfo.Gender,              // 性别
            userInfo.Age,                 // 年龄
            userInfo.BirthDate,           // 出生日期
            userInfo.OrganizationUnitId,  // 组织单位ID
            userInfo.OrganizationUnitName // 组织单位名称
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

