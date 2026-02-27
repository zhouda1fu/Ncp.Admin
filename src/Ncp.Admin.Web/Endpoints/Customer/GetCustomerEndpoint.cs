using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 获取客户详情请求
/// </summary>
/// <param name="Id">客户 ID</param>
public record GetCustomerRequest(CustomerId Id);

/// <summary>
/// 获取客户详情
/// </summary>
/// <param name="query">客户查询</param>
public class GetCustomerEndpoint(CustomerQuery query) : Endpoint<GetCustomerRequest, ResponseData<CustomerDetailDto>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetCustomerRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(req.Id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
