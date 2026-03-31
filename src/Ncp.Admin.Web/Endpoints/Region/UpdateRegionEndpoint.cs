using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Web.Application.Commands.RegionModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Region;

/// <summary>
/// 更新区域请求
/// </summary>
/// <param name="Id">区域 ID</param>
/// <param name="Name">区域名称</param>
/// <param name="ParentId">父级区域 ID</param>
/// <param name="Level">层级</param>
/// <param name="SortOrder">排序</param>
public record UpdateRegionRequest(RegionId Id, string Name, RegionId ParentId, int Level, int SortOrder);

/// <summary>
/// 更新区域
/// </summary>
public class UpdateRegionEndpoint(IMediator mediator)
    : Endpoint<UpdateRegionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Region");
        Put("/api/admin/regions/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RegionEdit);
    }

    public override async Task HandleAsync(UpdateRegionRequest req, CancellationToken ct)
    {
        var cmd = new UpdateRegionCommand(req.Id, req.Name, req.ParentId, req.Level, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
