using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.PositionEndpoints;

/// <summary>
/// 创建岗位的请求模型
/// </summary>
public record CreatePositionRequest(string Name, string Code, string Description, DeptId DeptId, int SortOrder, int Status);

/// <summary>
/// 创建岗位的响应模型
/// </summary>
public record CreatePositionResponse(PositionId Id, string Name, string Code);

/// <summary>
/// 创建岗位
/// </summary>
public class CreatePositionEndpoint(IMediator mediator) : Endpoint<CreatePositionRequest, ResponseData<CreatePositionResponse>>
{
    public override void Configure()
    {
        Tags("Positions");
        Description(b => b.AutoTagOverride("Positions"));
        Post("/api/admin/position");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.PositionCreate);
    }

    public override async Task HandleAsync(CreatePositionRequest req, CancellationToken ct)
    {
        var cmd = new CreatePositionCommand(req.Name, req.Code, req.Description, req.DeptId, req.SortOrder, req.Status);
        var positionId = await mediator.Send(cmd, ct);
        var response = new CreatePositionResponse(positionId, req.Name, req.Code);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
