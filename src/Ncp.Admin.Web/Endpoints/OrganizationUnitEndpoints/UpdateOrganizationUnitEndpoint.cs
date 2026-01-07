using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Web.Application.Commands.OrganizationUnitCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 更新组织单位的请求模型
/// </summary>
/// <param name="Id">OrganizationUnitId</param>
/// <param name="Name">组织单位名称</param>
/// <param name="Description">组织单位描述</param>
/// <param name="ParentId">父级组织单位ID，可为空表示顶级组织</param>
/// <param name="SortOrder">排序顺序</param>
public record UpdateOrganizationUnitRequest(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId? ParentId, int SortOrder);

/// <summary>
/// 更新组织单位的API端点
/// 该端点用于修改现有组织单位的基本信息
/// </summary>
[Tags("OrganizationUnits")]
public class UpdateOrganizationUnitEndpoint(IMediator mediator) : Endpoint<UpdateOrganizationUnitRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新组织单位信息
        Put("/api/organization-units");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和组织单位编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 将请求转换为命令，通过中介者发送，并返回更新结果
    /// </summary>
    /// <param name="req">包含组织单位更新信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateOrganizationUnitRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        // 如果父级ID为空，则设置为根组织单位（ID为0）
        var command = new UpdateOrganizationUnitCommand(
            req.Id,                                    // 组织单位ID
            req.Name,                                  // 组织单位名称
            req.Description,                           // 组织单位描述
            req.ParentId ?? new OrganizationUnitId(0), // 父级组织单位ID，默认为根组织
            req.SortOrder                              // 排序顺序
        );

        // 通过中介者发送命令，执行实际的业务逻辑
        await mediator.Send(command, ct);

        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

