using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 更新部门的请求模型
/// </summary>
/// <param name="Id">部门ID</param>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
/// <param name="ParentId">父级部门ID，可为空表示顶级部门</param>
/// <param name="Status">状态（0=禁用，1=启用）</param>
/// <param name="SortOrder">排序号</param>
/// <param name="ResponsibleUserIds">部门负责人用户 ID 列表</param>
/// <param name="DefaultResponsibleUserId">默认负责人用户 ID；仅用于单人兜底场景</param>
public record UpdateDeptRequest(
    DeptId Id,
    string Name,
    string Remark,
    DeptId? ParentId,
    int Status,
    int SortOrder = 0,
    IReadOnlyList<UserId>? ResponsibleUserIds = null,
    UserId? DefaultResponsibleUserId = null);

/// <summary>
/// 更新部门
/// </summary>
/// <param name="mediator"></param>
public class UpdateDeptEndpoint(IMediator mediator) : Endpoint<UpdateDeptRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Depts");
        Description(b => b.AutoTagOverride("Depts").WithSummary("更新部门"));
        Put("/api/admin/dept");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.DeptEdit);
    }

    public override async Task HandleAsync(UpdateDeptRequest req, CancellationToken ct)
    {
        // 如果父级ID为空，则设置为根部门（ID为0）
        var command = new UpdateDeptCommand(
            req.Id,
            req.Name,
            req.Remark,
            req.ParentId ?? DeptId.Unassigned,
            req.Status,
            req.SortOrder,
            req.ResponsibleUserIds ?? [],
            req.DefaultResponsibleUserId
        );
        await mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
