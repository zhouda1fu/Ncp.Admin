using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class GetCustomerListRequest : CustomerQueryInput { }

public class GetCustomerListEndpoint(CustomerQuery query)
    : Endpoint<GetCustomerListRequest, ResponseData<PagedData<CustomerQueryDto>>>
{
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
    }

    public override async Task HandleAsync(GetCustomerListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
