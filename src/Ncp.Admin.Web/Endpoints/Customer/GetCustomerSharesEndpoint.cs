using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 获取客户共享列表请求
/// </summary>
/// <param name="Id">客户 ID</param>
public record GetCustomerSharesRequest(CustomerId Id);

/// <summary>
/// 获取客户共享列表响应
/// </summary>
/// <param name="SharedToUserIds">共享给用户 ID 列表</param>
public record GetCustomerSharesResponse(IReadOnlyList<UserId> SharedToUserIds);

/// <summary>
/// 获取客户共享列表
/// </summary>
/// <param name="query">客户查询</param>
public class GetCustomerSharesEndpoint(CustomerQuery query)
    : Endpoint<GetCustomerSharesRequest, ResponseData<GetCustomerSharesResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers/{id}/shares");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerShare);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetCustomerSharesRequest req, CancellationToken ct)
    {
        var ids = await query.GetSharedToUserIdsAsync(req.Id, ct);
        await Send.OkAsync(new GetCustomerSharesResponse(ids).AsResponseData(), cancellation: ct);
    }
}

