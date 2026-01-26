using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 删除部门的请求模型
/// </summary>
/// <param name="Id">部门ID</param>
public record DeleteDeptRequest(DeptId Id);

/// <summary>
/// 删除部门
/// </summary>
/// <param name="mediator"></param>
public class DeleteDeptEndpoint(IMediator mediator) : Endpoint<DeleteDeptRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Depts");
        Description(b => b.AutoTagOverride("Depts"));
        Delete("/api/admin/dept/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptDelete);
    }

    public override async Task HandleAsync(DeleteDeptRequest request, CancellationToken ct)
    {
        await mediator.Send(new DeleteDeptCommand(request.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
