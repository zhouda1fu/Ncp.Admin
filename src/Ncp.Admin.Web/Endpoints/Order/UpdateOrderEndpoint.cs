using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Order;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

public record UpdateOrderRequest(
    string CustomerName,
    ProjectId ProjectId,
    string OrderNumber,
    OrderType Type,
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
    OrderInvoiceTypeOptionId InvoiceTypeId,
    decimal InstallationFee,
    decimal EstimatedFreight,
    IReadOnlyList<OrderContractFileItem> ContractFiles,
    IReadOnlyList<OrderContractFileItem> StockFiles,
    SelectedContractFileId SelectedContractFileId,
    bool IsShipped,
    PaymentStatus PaymentStatus,
    bool ContractNotCompanyTemplate,
    decimal ContractAmount,
    string ReceiverName,
    string ReceiverPhone,
    string ReceiverAddress,
    DateTimeOffset PayDate,
    DateTimeOffset DeliveryDate,
    string? OrderLogisticsCompanyId,
    string? OrderLogisticsMethodId,
    LogisticsPaymentMethodId LogisticsPaymentMethodId,
    string WaybillNumber,
    decimal ShippingFee,
    bool ShippingFeeIsPay,
    decimal Surcharge,
    bool IsNoLogo,
    string AfterSalesServiceId,
    bool IsAssess,
    string Comments,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool IsRed,
    bool IsFree,
    bool IsRepay,
    DateTimeOffset RepayDate,
    DateTimeOffset FRepayDate,
    DateTimeOffset DelayDate,
    string DelayReason,
    string Feedback,
    string Scontent,
    WarehouseStatus WarehouseStatus,
    IReadOnlyList<OrderCategoryContractItem>? OrderCategories,
    IReadOnlyList<UpdateOrderItemDto> Items);

public class UpdateOrderEndpoint(IMediator mediator)
    : Endpoint<UpdateOrderRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Put("/api/admin/orders/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderEdit);
    }

    public override async Task HandleAsync(UpdateOrderRequest req, CancellationToken ct)
    {
        var id = new OrderId(Route<Guid>("id"));
        var contractFilesJson = req.ContractFiles != null ? JsonSerializer.Serialize(req.ContractFiles) : "[]";
        var stockFilesJson = req.StockFiles != null ? JsonSerializer.Serialize(req.StockFiles) : "[]";
        var orderLogisticsCompanyId = ParseOrderLogisticsCompanyId(req.OrderLogisticsCompanyId);
        var orderLogisticsMethodId = ParseOrderLogisticsMethodId(req.OrderLogisticsMethodId);
        await mediator.Send(new UpdateOrderCommand(
            id,
            req.CustomerName,
            req.ProjectId,
            req.OrderNumber,
            req.Type,
            req.Amount,
            req.Remark,
            req.OwnerId,
            req.OwnerName,
            req.DeptId,
            req.DeptName,
            req.ProjectContactName,
            req.ProjectContactPhone,
            req.Warranty,
            req.ContractSigningCompany,
            req.ContractTrustee,
            req.NeedInvoice,
            req.InvoiceTypeId,
            req.InstallationFee,
            req.EstimatedFreight,
            contractFilesJson,
            stockFilesJson,
            req.SelectedContractFileId,
            req.IsShipped,
            req.PaymentStatus,
            req.ContractNotCompanyTemplate,
            req.ContractAmount,
            req.ReceiverName ?? string.Empty,
            req.ReceiverPhone ?? string.Empty,
            req.ReceiverAddress ?? string.Empty,
            req.PayDate,
            req.DeliveryDate,
            orderLogisticsCompanyId,
            orderLogisticsMethodId,
            req.LogisticsPaymentMethodId,
            req.WaybillNumber ?? string.Empty,
            req.ShippingFee,
            req.ShippingFeeIsPay,
            req.Surcharge,
            req.IsNoLogo,
            req.AfterSalesServiceId,
            req.IsAssess,
            req.Comments,
            req.StartDate,
            req.EndDate,
            req.IsRed,
            req.IsFree,
            req.IsRepay,
            req.RepayDate,
            req.FRepayDate,
            req.DelayDate,
            req.DelayReason,
            req.Feedback,
            req.Scontent,
            req.WarehouseStatus,
            req.OrderCategories ?? [],
            req.Items), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }

    private static OrderLogisticsCompanyId ParseOrderLogisticsCompanyId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new OrderLogisticsCompanyId(Guid.Empty);
        }

        if (!Guid.TryParse(value, out var id))
        {
            throw new KnownException("物流公司ID格式不正确", ErrorCodes.OrderLogisticsCompanyIdInvalid);
        }

        return new OrderLogisticsCompanyId(id);
    }

    private static OrderLogisticsMethodId ParseOrderLogisticsMethodId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new OrderLogisticsMethodId(Guid.Empty);
        }

        if (!Guid.TryParse(value, out var id))
        {
            throw new KnownException("物流方式ID格式不正确", ErrorCodes.OrderLogisticsMethodIdInvalid);
        }

        return new OrderLogisticsMethodId(id);
    }
}
