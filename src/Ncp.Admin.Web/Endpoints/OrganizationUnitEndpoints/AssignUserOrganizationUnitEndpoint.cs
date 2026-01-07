using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 分配用户组织架构的请求模型
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="OrganizationUnitId">组织架构ID</param>
/// <param name="OrganizationUnitName">组织架构名称</param>
public record AssignUserOrganizationUnitRequest(UserId UserId, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName);

/// <summary>
/// 分配用户组织架构的API端点
/// 该端点用于将用户分配到指定的组织架构
/// </summary>
[Tags("OrganizationUnits")]
public class AssignUserOrganizationUnitEndpoint(IMediator mediator) : Endpoint<AssignUserOrganizationUnitRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于分配用户组织架构
        Post("/api/organization-units/assign-user");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和组织单位分配权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitAssign);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，执行分配操作
    /// </summary>
    /// <param name="req">包含用户ID和组织架构信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(AssignUserOrganizationUnitRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        var command = new AssignUserOrganizationUnitCommand(
            req.UserId,                // 用户ID
            req.OrganizationUnitId,    // 组织架构ID
            req.OrganizationUnitName   // 组织架构名称
        );

        // 通过中介者发送命令，执行实际的分配业务逻辑
        await mediator.Send(command, ct);

        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

