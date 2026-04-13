using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 获取用户的公海片区分配授权审计分页列表请求
/// </summary>
public class GetCustomerSeaRegionAssignUserAuditsRequest : CustomerSeaRegionAssignAuditsQueryInput
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public UserId UserId { get; set; } = default!;
}

public class GetCustomerSeaRegionAssignUserAuditsEndpoint(CustomerSeaRegionAssignmentQuery query)
    : Endpoint<GetCustomerSeaRegionAssignUserAuditsRequest, ResponseData<PagedData<CustomerSeaRegionAssignAuditListItemDto>>>
{
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customer-sea-region-assign/users/{userId}/audits");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaRegionAssignView);
        Description(b => b.AutoTagOverride("Customer").WithSummary("获取用户的公海片区授权审计记录"));
    }

    public override async Task HandleAsync(GetCustomerSeaRegionAssignUserAuditsRequest req, CancellationToken ct)
    {
        var data = await query.GetUserAuditsPagedAsync(req.UserId, req, ct);
        await Send.OkAsync(data.AsResponseData(), cancellation: ct);
    }
}

