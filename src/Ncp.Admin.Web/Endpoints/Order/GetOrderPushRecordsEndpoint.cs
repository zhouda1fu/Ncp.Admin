using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

/// <summary>
/// 获取订单推送记录端点
/// </summary>
public class GetOrderPushRecordsEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<List<OrderPushRecordResponse>>>
{
    public override void Configure()
    {
        Get("/api/admin/orders/{id}/push-records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Tags("Order");
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = new OrderId(Route<Guid>("id"));
        var result = await mediator.Send(new GetOrderPushRecordsQuery(orderId), ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}