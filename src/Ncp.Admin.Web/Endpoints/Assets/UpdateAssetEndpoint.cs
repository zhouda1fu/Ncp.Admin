using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Web.Application.Commands.Assets;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Assets;

/// <summary>
/// 更新资产请求
/// </summary>
/// <param name="Id">资产 ID</param>
/// <param name="Name">名称</param>
/// <param name="Category">分类</param>
/// <param name="Code">编码</param>
/// <param name="PurchaseDate">购置日期</param>
/// <param name="Value">价值</param>
/// <param name="Remark">备注</param>
public record UpdateAssetRequest(AssetId Id, string Name, string Category, string Code, DateTimeOffset PurchaseDate, decimal Value, string? Remark);

public class UpdateAssetEndpoint(IMediator mediator) : Endpoint<UpdateAssetRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Asset");
        Description(b => b.AutoTagOverride("Asset").WithSummary("更新资产请求"));
        Put("/api/admin/assets/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetEdit);
    }

    public override async Task HandleAsync(UpdateAssetRequest req, CancellationToken ct)
    {
        var cmd = new UpdateAssetCommand(req.Id, req.Name, req.Category, req.Code, req.PurchaseDate, req.Value, req.Remark);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
