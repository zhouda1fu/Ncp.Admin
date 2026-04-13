using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.Customers;

public class GetCustomerSeaRegionAssignUsersRequest : CustomerSeaRegionAssignUsersQueryInput { }

public class GetCustomerSeaRegionAssignUsersEndpoint(CustomerSeaRegionAssignmentQuery query)
    : Endpoint<GetCustomerSeaRegionAssignUsersRequest, ResponseData<PagedData<CustomerSeaRegionAssignUserListItemDto>>>
{
    public override void Configure()
    {
        Tags("Customer");
        Description(b => b.AutoTagOverride("Customer").WithSummary("获取公海片区分配用户列表"));
        Get("/api/admin/customer-sea-region-assign/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaRegionAssignView);
    }

    public override async Task HandleAsync(GetCustomerSeaRegionAssignUsersRequest req, CancellationToken ct)
    {
        var data = await query.GetAssignUsersPagedAsync(req, ct);
        await Send.OkAsync(data.AsResponseData(), cancellation: ct);
    }
}

