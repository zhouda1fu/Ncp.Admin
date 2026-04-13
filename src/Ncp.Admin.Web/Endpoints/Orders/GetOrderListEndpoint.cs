using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 获取订单分页列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="OrderNumber">订单号</param>
/// <param name="CustomerId">客户</param>
/// <param name="Type">订单类型</param>
/// <param name="Status">状态</param>
/// <param name="CreatedFrom">创建时间起</param>
/// <param name="CreatedTo">创建时间止</param>
public record GetOrderListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? OrderNumber = null,
    CustomerId? CustomerId = null,
    OrderType? Type = null,
    OrderStatus? Status = null,
    DateTimeOffset? CreatedFrom = null,
    DateTimeOffset? CreatedTo = null);

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
        Description(b => b.AutoTagOverride("Order").WithSummary("获取订单分页列表"));
        Get("/api/admin/orders");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetOrderListRequest req, CancellationToken ct)
    {
        var input = new OrderQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            OrderNumber = req.OrderNumber,
            CustomerId = req.CustomerId,
            Type = req.Type,
            Status = req.Status,
            CreatedFrom = req.CreatedFrom,
            CreatedTo = req.CreatedTo,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
