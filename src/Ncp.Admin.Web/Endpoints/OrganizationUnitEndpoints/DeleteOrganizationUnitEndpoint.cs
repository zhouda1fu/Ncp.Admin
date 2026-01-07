using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 删除组织单位的请求模型
/// </summary>
/// <param name="Id">要删除的组织单位ID</param>
public record DeleteOrganizationUnitRequest(OrganizationUnitId Id);

/// <summary>
/// 删除组织单位的API端点
/// 该端点用于删除指定的组织单位（软删除）
/// </summary>
[Tags("OrganizationUnits")]
public class DeleteOrganizationUnitEndpoint(IMediator mediator) : Endpoint<DeleteOrganizationUnitRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP DELETE方法，用于删除组织单位
        Delete("/api/organization-units/{id}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和组织单位删除权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitDelete);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，执行删除操作
    /// </summary>
    /// <param name="req">包含要删除的组织单位ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(DeleteOrganizationUnitRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        var command = new DeleteOrganizationUnitCommand(req.Id);

        // 通过中介者发送命令，执行实际的业务逻辑（软删除）
        await mediator.Send(command, ct);

        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

