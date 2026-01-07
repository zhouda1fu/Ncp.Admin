using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Web.Application.Commands.UserCommands;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 更新用户信息的请求模型
/// </summary>
/// <param name="UserId">要更新的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Phone">电话号码</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="BirthDate">出生日期</param>
/// <param name="OrganizationUnitId">组织单位ID</param>
/// <param name="OrganizationUnitName">组织单位名称</param>
/// <param name="Password">密码（可选，为空则不更新）</param>
public record UpdateUserRequest(UserId UserId, string Name, string Email, string Phone, string RealName, int Status, string Gender, int Age, DateTimeOffset BirthDate, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName, string Password);

/// <summary>
/// 更新用户信息的响应模型
/// </summary>
/// <param name="UserId">已更新的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
public record UpdateUserResponse(UserId UserId, string Name, string Email);

/// <summary>
/// 更新用户信息的API端点
/// 该端点用于修改现有用户的基本信息，包括个人信息、组织单位和密码
/// </summary>
[Tags("Users")]
public class UpdateUserEndpoint(IMediator mediator) : Endpoint<UpdateUserRequest, ResponseData<UpdateUserResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新用户信息
        Put("/api/user/update");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和用户编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 处理密码哈希，将请求转换为命令，通过中介者发送，并返回更新结果
    /// </summary>
    /// <param name="request">包含用户更新信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        // 初始化密码哈希变量
        var passwordHash = string.Empty;

        // 如果提供了新密码，则进行哈希处理
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            passwordHash = PasswordHasher.HashPassword(request.Password);
        }

        // 创建更新用户命令对象，包含所有用户信息
        var cmd = new UpdateUserCommand(
            request.UserId,              // 用户ID
            request.Name,                // 用户名
            request.Email,               // 邮箱
            request.Phone,               // 电话
            request.RealName,            // 真实姓名
            request.Status,              // 状态
            request.Gender,              // 性别
            request.Age,                 // 年龄
            request.BirthDate,           // 出生日期
            request.OrganizationUnitId,  // 组织单位ID
            request.OrganizationUnitName, // 组织单位名称
            passwordHash                 // 密码哈希（如果提供了新密码）
        );

        // 通过中介者发送命令，执行实际的更新业务逻辑
        // 返回更新后的用户ID
        var userId = await mediator.Send(cmd, ct);

        // 创建响应对象，包含更新后的用户信息
        var response = new UpdateUserResponse(
            userId,        // 用户ID
            request.Name,  // 用户名
            request.Email  // 邮箱
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

