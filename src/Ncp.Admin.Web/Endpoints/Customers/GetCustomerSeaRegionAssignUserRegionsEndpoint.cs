using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;
using NetCorePal.Extensions.Primitives;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 获取用户的公海片区分配片区列表请求
/// </summary>
/// <param name="UserId">用户 ID</param>
public record GetCustomerSeaRegionAssignUserRegionsRequest(UserId UserId);

public class GetCustomerSeaRegionAssignUserRegionsEndpoint(
    CustomerSeaRegionAssignmentQuery query)
    : Endpoint<GetCustomerSeaRegionAssignUserRegionsRequest, ResponseData<IReadOnlyList<RegionId>>>
{
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customer-sea-region-assign/users/{userId}/regions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaRegionAssignView);
        Description(b => b.AutoTagOverride("Customer").WithSummary("获取用户的公海片区列表"));
    }

    public override async Task HandleAsync(GetCustomerSeaRegionAssignUserRegionsRequest req, CancellationToken ct)
    {
        var regions = await query.GetUserRegionsAsync(req.UserId, ct);
        await Send.OkAsync(regions.AsResponseData(), cancellation: ct);
    }
}

