using FastEndpoints;
using FastEndpoints.Swagger;
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
/// 获取部门
/// </summary>
/// <param name="deptQuery"></param>
public class GetDeptEndpoint(DeptQuery deptQuery) : Endpoint<GetDeptRequest, ResponseData<GetDeptResponse>>
{
    public override void Configure()
    {
        Tags("Depts");
        Description(b => b.AutoTagOverride("Depts"));
        Get("/api/admin/dept/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptView);
    }

    public override async Task HandleAsync(GetDeptRequest req, CancellationToken ct)
    {
        var dept = await deptQuery.GetDeptByIdAsync(req.Id, ct);
        if (dept == null)
            await Send.NotFoundAsync(ct);
        else
            await Send.OkAsync(new GetDeptResponse(dept.Id, dept.Name, dept.Remark, dept.ParentId, dept.Status, dept.CreatedAt).AsResponseData(), cancellation: ct);
    }
}
