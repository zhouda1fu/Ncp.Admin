using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 重排同级部门排序请求
/// </summary>
/// <param name="ParentId">父级部门 ID；未分配表示顶级部门</param>
/// <param name="OrderedIds">同级部门按新顺序排列的 ID 列表</param>
public record ReorderDeptSortRequest(DeptId? ParentId, IReadOnlyList<DeptId> OrderedIds);

/// <summary>
/// 重排同级部门排序
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class ReorderDeptSortEndpoint(IMediator mediator)
    : Endpoint<ReorderDeptSortRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Depts");
        Description(b => b.AutoTagOverride("Depts").WithSummary("重排同级部门排序"));
        Post("/api/admin/dept/reorder");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(ReorderDeptSortRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new ReorderDeptSortCommand(req.ParentId ?? DeptId.Unassigned, req.OrderedIds),
            ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
