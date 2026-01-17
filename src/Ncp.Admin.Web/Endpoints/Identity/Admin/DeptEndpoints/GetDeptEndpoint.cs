using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 获取单个部门的请求模型
/// </summary>
/// <param name="Id">部门ID</param>
public record GetDeptRequest(DeptId Id);

/// <summary>
/// 获取单个部门的响应模型
/// </summary>
/// <param name="Id">部门ID</param>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
/// <param name="ParentId">父级部门ID</param>
/// <param name="Status">状态（0=禁用，1=启用）</param>
/// <param name="CreatedAt">创建时间</param>
public record GetDeptResponse(DeptId Id, string Name, string Remark, DeptId ParentId, int Status, DateTimeOffset CreatedAt);

/// <summary>
/// 获取单个部门的API端点
/// 该端点用于根据ID查询特定部门的详细信息
/// </summary>
[Tags("Depts")]
public class GetDeptEndpoint(DeptQuery deptQuery) : Endpoint<GetDeptRequest, ResponseData<GetDeptResponse?>>
{
   
    public override void Configure()
    {
      
        Get("/api/admin/dept/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptView);
    }

    public override async Task HandleAsync(GetDeptRequest req, CancellationToken ct)
    {
        // 从路由参数中获取部门ID（如果请求对象中没有）
        DeptId deptId;
        if (req.Id != default)
        {
            deptId = req.Id;
        }
        else
        {
            var idString = Route<string>("id");
            if (string.IsNullOrEmpty(idString) || !long.TryParse(idString, out var idValue))
            {
                throw new KnownException("无效的部门ID");
            }
            deptId = new DeptId(idValue);
        }

        // 通过查询服务获取部门详细信息
        var dept = await deptQuery.GetDeptByIdAsync(deptId, ct);

        // 验证部门是否存在
        if (dept == null)
        {
            throw new KnownException($"未找到部门，Id = {deptId}");
        }

        // 创建响应对象
        var response = new GetDeptResponse(
            dept.Id,
            dept.Name,
            dept.Remark,
            dept.ParentId,
            dept.Status,
            dept.CreatedAt
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
