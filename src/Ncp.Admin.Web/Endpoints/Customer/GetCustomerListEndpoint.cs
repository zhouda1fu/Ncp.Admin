using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 获取客户分页列表请求（继承客户查询入参）
/// </summary>
public class GetCustomerListRequest : CustomerQueryInput { }

/// <summary>
/// 获取客户分页列表
/// </summary>
/// <param name="query">客户查询</param>
public class GetCustomerListEndpoint(CustomerQuery query)
    : Endpoint<GetCustomerListRequest, ResponseData<PagedData<CustomerQueryDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetCustomerListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
