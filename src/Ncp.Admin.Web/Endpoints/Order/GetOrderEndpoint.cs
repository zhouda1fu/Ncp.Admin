using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

/// <summary>
/// 获取订单详情请求
/// </summary>
/// <param name="Id">订单 ID</param>
public record GetOrderRequest(OrderId Id);

/// <summary>
/// 获取订单详情
/// </summary>
/// <param name="query">订单查询</param>
public class GetOrderEndpoint(OrderQuery query) : Endpoint<GetOrderRequest, ResponseData<OrderDetailDto>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Get("/api/admin/orders/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetOrderRequest req, CancellationToken ct)
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
