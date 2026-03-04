using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

/// <summary>
/// 获取订单分页列表请求（继承订单查询入参）
/// </summary>
public class GetOrderListRequest : OrderQueryInput { }

/// <summary>
/// 获取订单分页列表
/// </summary>
/// <param name="query">订单查询</param>
public class GetOrderListEndpoint(OrderQuery query)
    : Endpoint<GetOrderListRequest, ResponseData<PagedData<OrderQueryDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Get("/api/admin/orders");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetOrderListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
