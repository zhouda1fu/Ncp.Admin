using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Order;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

/// <summary>
/// 获取订单备注列表请求
/// </summary>
/// <param name="Id">订单 ID</param>
public record GetOrderRemarksRequest(OrderId Id);

/// <summary>
/// 获取订单备注列表
/// </summary>
public class GetOrderRemarksEndpoint(IMediator mediator) : Endpoint<GetOrderRemarksRequest, ResponseData<List<OrderRemarkDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Get("/api/admin/orders/{id}/remarks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetOrderRemarksRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderRemarksQuery(req.Id), ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}

