using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 删除订单备注请求
/// </summary>
/// <param name="Id">订单 ID</param>
/// <param name="RemarkId">备注 ID</param>
public record DeleteOrderRemarkRequest(OrderId Id, OrderRemarkId RemarkId);

/// <summary>
/// 删除订单备注
/// </summary>
public class DeleteOrderRemarkEndpoint(IMediator mediator)
    : Endpoint<DeleteOrderRemarkRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("删除订单备注"));
        Delete("/api/admin/orders/{id}/remarks/{remarkId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderContractAmount);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(DeleteOrderRemarkRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeleteOrderRemarkCommand(req.Id, req.RemarkId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

