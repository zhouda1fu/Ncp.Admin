using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 客户搜索请求（继承客户搜索入参，用于弹窗等场景）
/// </summary>
public class GetCustomerSearchRequest : CustomerSearchInput { }

/// <summary>
/// 客户搜索（分页）
/// </summary>
/// <param name="query">客户查询</param>
public class GetCustomerSearchEndpoint(CustomerQuery query)
    : Endpoint<GetCustomerSearchRequest, ResponseData<PagedData<CustomerSearchDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers/search");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetCustomerSearchRequest req, CancellationToken ct)
    {
        var result = await query.SearchAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
