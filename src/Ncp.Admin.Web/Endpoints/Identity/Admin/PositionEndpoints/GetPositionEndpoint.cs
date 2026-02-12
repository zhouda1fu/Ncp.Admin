using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.PositionEndpoints;

/// <summary>
/// 获取单个岗位的请求模型
/// </summary>
public record GetPositionRequest(PositionId Id);

/// <summary>
/// 获取单个岗位的响应模型
/// </summary>
public record GetPositionResponse(
    PositionId Id, string Name, string Code, string Description,
    DeptId DeptId, string? DeptName, int SortOrder, int Status, DateTimeOffset CreatedAt);

/// <summary>
/// 获取岗位详情
/// </summary>
public class GetPositionEndpoint(PositionQuery positionQuery) : Endpoint<GetPositionRequest, ResponseData<GetPositionResponse>>
{
    public override void Configure()
    {
        Tags("Positions");
        Description(b => b.AutoTagOverride("Positions"));
        Get("/api/admin/position/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.PositionView);
    }

    public override async Task HandleAsync(GetPositionRequest req, CancellationToken ct)
    {
        var position = await positionQuery.GetPositionByIdAsync(req.Id, ct);
        if (position == null)
            await Send.NotFoundAsync(ct);
        else
            await Send.OkAsync(new GetPositionResponse(
                position.Id, position.Name, position.Code, position.Description,
                position.DeptId, position.DeptName, position.SortOrder, position.Status, position.CreatedAt
            ).AsResponseData(), cancellation: ct);
    }
}
