using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 提交订单审批请求
/// </summary>
/// <param name="Id">订单 ID</param>
/// <param name="Remark">备注</param>
public record SubmitOrderRequest(OrderId Id, string Remark = "");

/// <summary>
/// 提交订单审批（草稿或已驳回的订单可发起或重新发起审批流程）
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class SubmitOrderEndpoint(IMediator mediator) : Endpoint<SubmitOrderRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("提交订单审批（草稿或已驳回的订单可发起或重新发起审批流程）"));
        Post("/api/admin/orders/{id}/submit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderSubmit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(SubmitOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new SubmitOrderCommand(req.Id, req.Remark ?? ""), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
