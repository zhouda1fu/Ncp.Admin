using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Web.Application.Commands.Region;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Region;

/// <summary>
/// 创建区域请求
/// </summary>
/// <param name="Code">区域编码（作为区域 ID 值）</param>
/// <param name="Name">区域名称</param>
/// <param name="ParentId">父级区域 ID</param>
/// <param name="Level">层级</param>
/// <param name="SortOrder">排序</param>
public record CreateRegionRequest(long Code, string Name, RegionId ParentId, int Level, int SortOrder);

public record CreateRegionResponse(RegionId Id);

public class CreateRegionEndpoint(IMediator mediator)
    : Endpoint<CreateRegionRequest, ResponseData<CreateRegionResponse>>
{
    public override void Configure()
    {
        Tags("Region");
        Post("/api/admin/regions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RegionCreate);
    }

    public override async Task HandleAsync(CreateRegionRequest req, CancellationToken ct)
    {
        var cmd = new CreateRegionCommand(req.Code, req.Name, req.ParentId, req.Level, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateRegionResponse(id).AsResponseData(), cancellation: ct);
    }
}
