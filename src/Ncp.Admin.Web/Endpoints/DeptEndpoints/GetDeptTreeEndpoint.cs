using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.DeptEndpoints;

/// <summary>
/// 获取部门树的请求模型
/// </summary>
/// <param name="IncludeInactive">是否包含非激活的部门</param>
public record GetDeptTreeRequest(bool IncludeInactive = false);

/// <summary>
/// 获取部门树的API端点
/// 该端点用于查询系统中的部门树形结构
/// </summary>
[Tags("Depts")]
public class GetDeptTreeEndpoint(DeptQuery deptQuery) : Endpoint<GetDeptTreeRequest, ResponseData<IEnumerable<DeptTreeDto>>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询部门树
        Get("/api/dept/tree");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和部门查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 查询部门树形结构并返回结果
    /// </summary>
    /// <param name="req">部门树查询请求参数</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetDeptTreeRequest req, CancellationToken ct)
    {
        // 通过查询服务获取部门树
        var tree = await deptQuery.GetDeptTreeAsync(req.IncludeInactive, ct);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(tree.AsResponseData(), cancellation: ct);
    }
}
