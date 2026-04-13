using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Assets;

/// <summary>
/// 资产列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Code">资产编码</param>
/// <param name="Name">名称</param>
/// <param name="Status">状态</param>
public record GetAssetListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Code = null,
    string? Name = null,
    AssetStatus? Status = null);

public class GetAssetListEndpoint(AssetQuery query)
    : Endpoint<GetAssetListRequest, ResponseData<PagedData<AssetQueryDto>>>
{
    public override void Configure()
    {
        Tags("Asset");
        Description(b => b.AutoTagOverride("Asset").WithSummary("资产列表请求"));
        Get("/api/admin/assets");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetView);
    }

    public override async Task HandleAsync(GetAssetListRequest req, CancellationToken ct)
    {
        var input = new AssetQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Code = req.Code,
            Name = req.Name,
            Status = req.Status,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
