using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 更新订单备注请求
/// </summary>
/// <param name="Id">订单 ID</param>
/// <param name="RemarkId">备注 ID</param>
/// <param name="Content">备注内容</param>
public record UpdateOrderRemarkRequest(OrderId Id, OrderRemarkId RemarkId, string Content);

/// <summary>
/// 更新订单备注
/// </summary>
public class UpdateOrderRemarkEndpoint(IMediator mediator)
    : Endpoint<UpdateOrderRemarkRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("更新订单备注"));
        Put("/api/admin/orders/{id}/remarks/{remarkId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderContractAmount);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateOrderRemarkRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateOrderRemarkCommand(req.Id, req.RemarkId, req.Content ?? string.Empty), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

