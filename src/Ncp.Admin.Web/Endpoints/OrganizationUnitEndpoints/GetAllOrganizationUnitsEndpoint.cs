using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrganizationUnitEndpoints;

/// <summary>
/// 获取所有组织单位的API端点
/// 该端点用于查询系统中的所有组织单位信息
/// </summary>
[Tags("OrganizationUnits")]
public class GetAllOrganizationUnitsEndpoint(OrganizationUnitQuery organizationUnitQuery) : Endpoint<OrganizationUnitQueryInput, ResponseData<IEnumerable<OrganizationUnitQueryDto>>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询组织单位信息
        Get("/api/organization-units");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和组织单位查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrganizationUnitView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 查询所有组织单位信息并返回结果
    /// </summary>
    /// <param name="req">组织单位查询输入参数</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(OrganizationUnitQueryInput req, CancellationToken ct)
    {
        // 通过查询服务获取所有组织单位信息
        var organizationUnits = await organizationUnitQuery.GetAllOrganizationUnitsAsync(req, ct);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(organizationUnits.AsResponseData(), cancellation: ct);
    }
}

