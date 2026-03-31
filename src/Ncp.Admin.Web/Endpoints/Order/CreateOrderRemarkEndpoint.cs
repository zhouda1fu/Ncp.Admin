using System.Security.Claims;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Order;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

/// <summary>
/// 创建订单备注请求（固定写入 TypeId=0）
/// </summary>
/// <param name="Id">订单 ID</param>
/// <param name="Content">备注内容</param>
public record CreateOrderRemarkRequest(OrderId Id, string Content);

/// <summary>
/// 创建订单备注
/// </summary>
public class CreateOrderRemarkEndpoint(IMediator mediator) : Endpoint<CreateOrderRemarkRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Post("/api/admin/orders/{id}/remarks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderContractAmount);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateOrderRemarkRequest req, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = !string.IsNullOrWhiteSpace(userIdString) && long.TryParse(userIdString, out var userIdValue)
            ? new UserId(userIdValue)
            : new UserId(0);

        await mediator.Send(new CreateOrderRemarkCommand(req.Id, userId, req.Content ?? string.Empty), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

