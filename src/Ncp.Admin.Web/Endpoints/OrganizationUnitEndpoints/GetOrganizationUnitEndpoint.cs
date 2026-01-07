using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Queries.Orders;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Endpoints.OrderEndpoints;

namespace Ncp.Admin.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 获取单个组织单位的请求模型
/// </summary>
/// <param name="Id">组织单位ID</param>
public record GetOrganizationUnitRequest(OrganizationUnitId Id);

/// <summary>
/// 获取单个组织单位的响应模型
/// </summary>
/// <param name="Id">组织单位ID</param>
/// <param name="Name">组织单位名称</param>
/// <param name="Description">组织单位描述</param>
/// <param name="ParentId">父级组织单位ID</param>
/// <param name="SortOrder">排序顺序</param>
/// <param name="IsActive">是否激活</param>
/// <param name="CreatedAt">创建时间</param>
public record GetOrganizationUnitResponse(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId ParentId, int SortOrder, bool IsActive, DateTimeOffset CreatedAt);

/// <summary>
/// 获取单个组织单位的API端点
/// 该端点用于根据ID查询特定组织单位的详细信息
/// </summary>
[Tags("OrganizationUnits")]
public class GetOrganizationUnitEndpoint(OrganizationUnitQuery organizationUnitQuery) : Endpoint<GetOrganizationUnitRequest, ResponseData<GetOrganizationUnitResponse?>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，通过路由参数获取组织单位ID
        Get("/api/organization-units/{id}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和组织单位查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 根据组织单位ID查询详细信息并返回结果
    /// </summary>
    /// <param name="req">包含组织单位ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetOrganizationUnitRequest req, CancellationToken ct)
    {
        // 通过查询服务获取组织单位详细信息
        var organizationUnit = await organizationUnitQuery.GetOrganizationUnitByIdAsync(req.Id, ct);

        // 验证组织单位是否存在
        if (organizationUnit == null)
        {
            throw new KnownException($"未找到组织单位，Id = {req.Id}");
        }

        // 创建响应对象
        var response = new GetOrganizationUnitResponse(
            organizationUnit.Id,
            organizationUnit.Name,
            organizationUnit.Description,
            organizationUnit.ParentId,
            organizationUnit.SortOrder,
            organizationUnit.IsActive,
            organizationUnit.CreatedAt
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }

}

