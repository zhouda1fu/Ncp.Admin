using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.CustomerSource;

public class GetCustomerSourceListEndpoint(CustomerSourceQuery query) : EndpointWithoutRequest<ResponseData<IReadOnlyList<CustomerSourceDto>>>
{
    public override void Configure()
    {
        Tags("CustomerSource");
        Get("/api/admin/customer-sources");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSourceView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
