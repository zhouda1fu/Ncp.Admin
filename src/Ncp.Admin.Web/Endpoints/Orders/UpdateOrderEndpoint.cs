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
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 更新订单请求
/// </summary>
/// <param name="CustomerName">客户名称</param>
/// <param name="ProjectId">项目 ID</param>
/// <param name="OrderNumber">订单编号</param>
/// <param name="Type">订单类型</param>
/// <param name="Amount">金额</param>
/// <param name="Remark">备注</param>
/// <param name="OwnerId">负责人用户 ID</param>
/// <param name="OwnerName">负责人姓名</param>
/// <param name="DeptId">负责人部门 ID</param>
/// <param name="DeptName">负责人部门名称</param>
/// <param name="ProjectContactName">项目联系人姓名</param>
/// <param name="ProjectContactPhone">项目联系人电话</param>
/// <param name="Warranty">质保期</param>
/// <param name="ContractSigningCompany">合同签订公司</param>
/// <param name="ContractTrustee">合同委托方</param>
/// <param name="NeedInvoice">是否需要发票</param>
/// <param name="InvoiceTypeId">发票类型选项 ID</param>
/// <param name="InstallationFee">安装费</param>
/// <param name="EstimatedFreight">预估运费</param>
/// <param name="ContractFiles">合同文件列表</param>
/// <param name="StockFiles">备货文件列表</param>
/// <param name="SelectedContractFileId">选中的合同文件 ID</param>
/// <param name="IsShipped">是否已发货</param>
/// <param name="PaymentStatus">到款状态</param>
/// <param name="ContractNotCompanyTemplate">合同是否非公司模板</param>
/// <param name="ContractAmount">合同金额</param>
/// <param name="ReceiverName">收货人姓名</param>
/// <param name="ReceiverPhone">收货人电话</param>
/// <param name="ReceiverAddress">收货地址</param>
/// <param name="PayDate">付款日期</param>
/// <param name="DeliveryDate">发货日期</param>
/// <param name="OrderLogisticsCompanyId">物流公司 ID（Guid 字符串，可空）</param>
/// <param name="OrderLogisticsMethodId">物流方式 ID（Guid 字符串，可空）</param>
/// <param name="LogisticsPaymentMethodId">物流费用支付方式</param>
/// <param name="WaybillNumber">运单编号</param>
/// <param name="ShippingFee">运费</param>
/// <param name="ShippingFeeIsPay">是否付运费</param>
/// <param name="Surcharge">附加费</param>
/// <param name="IsNoLogo">是否无 Logo</param>
/// <param name="AfterSalesServiceId">售后服务 ID</param>
/// <param name="IsAssess">是否评价</param>
/// <param name="Comments">评价内容</param>
/// <param name="StartDate">开始日期</param>
/// <param name="EndDate">结束日期</param>
/// <param name="IsRed">是否红冲</param>
/// <param name="IsFree">是否免费</param>
/// <param name="IsRepay">是否回款</param>
/// <param name="RepayDate">回款日期</param>
/// <param name="FRepayDate">最终回款日期</param>
/// <param name="DelayDate">延迟日期</param>
/// <param name="DelayReason">延迟原因</param>
/// <param name="Feedback">客户反馈</param>
/// <param name="Scontent">服务内容</param>
/// <param name="WarehousePickerId">配货人用户 ID</param>
/// <param name="WarehouseTechId">仓库技术用户 ID</param>
/// <param name="WarehouseReviewerId">复核人用户 ID</param>
/// <param name="WarehouseStatus">仓库状态</param>
/// <param name="OrderCategories">按分类合同优惠行（可空）</param>
/// <param name="Items">订单明细</param>
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
    UserId WarehousePickerId,
    UserId WarehouseTechId,
    UserId WarehouseReviewerId,
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
        Description(b => b.AutoTagOverride("Order").WithSummary("更新订单"));
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
            req.WarehousePickerId,
            req.WarehouseTechId,
            req.WarehouseReviewerId,
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
