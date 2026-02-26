using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class GetCustomerSearchRequest : CustomerSearchInput { }

public class GetCustomerSearchEndpoint(CustomerQuery query)
    : Endpoint<GetCustomerSearchRequest, ResponseData<PagedData<CustomerSearchDto>>>
{
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers/search");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
    }

    public override async Task HandleAsync(GetCustomerSearchRequest req, CancellationToken ct)
    {
        var result = await query.SearchAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
