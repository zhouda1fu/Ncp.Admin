using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 获取用户在客户公海片区分配中的授权片区汇总请求
/// </summary>
/// <param name="UserId">用户 ID</param>
public record GetCustomerSeaRegionAssignAuthorizedRegionsRequest(UserId UserId);

public class GetCustomerSeaRegionAssignAuthorizedRegionsEndpoint(
    CustomerSeaRegionAssignmentQuery query)
    : Endpoint<GetCustomerSeaRegionAssignAuthorizedRegionsRequest, ResponseData<CustomerSeaRegionAssignAuthorizedRegionsSummaryDto>>
{
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customer-sea-region-assign/users/{userId}/authorized-regions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaRegionAssignView);
        Description(b => b.AutoTagOverride("Customer").WithSummary("获取用户授权的公海片区汇总"));
    }

    public override async Task HandleAsync(GetCustomerSeaRegionAssignAuthorizedRegionsRequest req, CancellationToken ct)
    {
        var result = await query.GetAuthorizedRegionsSummaryAsync(req.UserId, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}

