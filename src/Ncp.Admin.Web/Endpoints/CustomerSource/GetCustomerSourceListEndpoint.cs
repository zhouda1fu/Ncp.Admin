using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.CustomerSource;

/// <summary>
/// 获取客户来源列表请求（可选按场景过滤）。
/// </summary>
/// <param name="Scene">使用场景：sea=公海，list=客户列表；不传或空则返回全部</param>
public record GetCustomerSourceListRequest(string? Scene);

/// <summary>
/// 获取客户来源列表（支持按场景过滤）。
/// </summary>
public class GetCustomerSourceListEndpoint(CustomerSourceQuery query) : Endpoint<GetCustomerSourceListRequest, ResponseData<IReadOnlyList<CustomerSourceDto>>>
{
    public override void Configure()
    {
        Tags("CustomerSource");
        Get("/api/admin/customer-sources");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSourceView);
    }

    public override async Task HandleAsync(GetCustomerSourceListRequest req, CancellationToken ct)
    {
        CustomerSourceUsageScene? scene = null;
        if (!string.IsNullOrWhiteSpace(req.Scene))
        {
            scene = req.Scene.Trim().ToLowerInvariant() switch
            {
                "sea" => CustomerSourceUsageScene.Sea,
                "list" => CustomerSourceUsageScene.List,
                _ => null
            };
        }
        var result = await query.GetListAsync(scene, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
