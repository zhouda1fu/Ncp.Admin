using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

/// <summary>
/// 更新订单时传入的明细行（所有字段必填）
/// </summary>
public record UpdateOrderRequestItemDto(
    ProductId ProductId,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);

/// <summary>
/// 更新订单请求
/// </summary>
public record UpdateOrderRequest(
    OrderId Id,
    string CustomerName,
    ProjectId ProjectId,
    ContractId ContractId,
    string OrderNumber,
    OrderType Type,
    OrderStatus Status,
    decimal Amount,
    string Remark,
    UserId OwnerId,
    string OwnerName,
    DeptId DeptId,
    string DeptName,
    string ProjectContactName,
    string ProjectContactPhone,
    string Warranty,
    string ContractSigningCompany,
    string ContractTrustee,
    bool NeedInvoice,
    decimal InstallationFee,
    decimal EstimatedFreight,
    string SelectedContractFileId,
    bool IsShipped,
    string PaymentStatus,
    bool ContractNotCompanyTemplate,
    decimal ContractDiscount,
    decimal ContractAmount,
    string ReceiverName,
    string ReceiverPhone,
    string ReceiverAddress,
    DateTimeOffset PayDate,
    DateTimeOffset DeliveryDate,
    IReadOnlyList<UpdateOrderRequestItemDto> Items,
    IReadOnlyList<OrderContractFileItem>? ContractFiles);

/// <summary>
/// 更新订单
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateOrderEndpoint(IMediator mediator) : Endpoint<UpdateOrderRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Put("/api/admin/orders");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateOrderRequest req, CancellationToken ct)
    {
        var items = req.Items.Select(x => new UpdateOrderItemDto(
            x.ProductId, x.ProductName, x.Model, x.Number, x.Qty, x.Unit, x.Price, x.Amount, x.Remark)).ToList();
        var contractFilesJson = JsonSerializer.Serialize(req.ContractFiles ?? [], new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await mediator.Send(new UpdateOrderCommand(
            req.Id,
            req.CustomerName ?? string.Empty,
            req.ProjectId,
            req.ContractId,
            req.OrderNumber,
            req.Type,
            req.Status,
            req.Amount,
            req.Remark ?? string.Empty,
            req.OwnerId,
            req.OwnerName ?? string.Empty,
            req.DeptId,
            req.DeptName ?? string.Empty,
            req.ProjectContactName ?? string.Empty,
            req.ProjectContactPhone ?? string.Empty,
            req.Warranty ?? string.Empty,
            req.ContractSigningCompany ?? string.Empty,
            req.ContractTrustee ?? string.Empty,
            req.NeedInvoice,
            req.InstallationFee,
            req.EstimatedFreight,
            contractFilesJson,
            req.SelectedContractFileId ?? string.Empty,
            req.IsShipped,
            req.PaymentStatus ?? string.Empty,
            req.ContractNotCompanyTemplate,
            req.ContractDiscount,
            req.ContractAmount,
            req.ReceiverName ?? string.Empty,
            req.ReceiverPhone ?? string.Empty,
            req.ReceiverAddress ?? string.Empty,
            req.PayDate,
            req.DeliveryDate,
            items), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
