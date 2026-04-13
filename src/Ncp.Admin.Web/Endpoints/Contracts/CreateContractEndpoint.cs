using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Contracts;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contracts;

/// <summary>
/// 创建合同请求（全部必传，ID 使用强类型）
/// </summary>
/// <param name="Code">合同编号</param>
/// <param name="Title">合同标题</param>
/// <param name="PartyA">甲方</param>
/// <param name="PartyB">乙方</param>
/// <param name="Amount">合同金额</param>
/// <param name="StartDate">开始日期</param>
/// <param name="EndDate">结束日期</param>
/// <param name="FileStorageKey">附件存储 Key</param>
/// <param name="OrderId">关联订单 ID</param>
/// <param name="CustomerId">关联客户 ID</param>
/// <param name="ContractType">合同类型（TypeValue）</param>
/// <param name="IncomeExpenseType">收支类型（TypeValue）</param>
/// <param name="SignDate">签约日期</param>
/// <param name="Note">备注</param>
/// <param name="Description">合同内容/描述</param>
/// <param name="DepartmentId">部门 ID</param>
/// <param name="BusinessManager">业务经理</param>
/// <param name="ResponsibleProject">负责项目</param>
/// <param name="InputCustomer">录入客户</param>
/// <param name="NextPaymentReminder">下次收付款报警</param>
/// <param name="ContractExpiryReminder">合同过期报警</param>
/// <param name="SingleDoubleSeal">单双章（0=单章 1=双章）</param>
/// <param name="InvoicingInformation">开票信息</param>
/// <param name="PaymentStatus">到款情况（TypeValue）</param>
/// <param name="WarrantyPeriod">质保期</param>
/// <param name="IsInstallmentPayment">是否分期</param>
/// <param name="AccumulatedAmount">累计金额</param>
public record CreateContractRequest(
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string FileStorageKey,
    OrderId OrderId,
    CustomerId CustomerId,
    int ContractType,
    int IncomeExpenseType,
    DateTimeOffset SignDate,
    string Note,
    string Description,
    DeptId DepartmentId,
    string BusinessManager,
    string ResponsibleProject,
    string InputCustomer,
    bool NextPaymentReminder,
    bool ContractExpiryReminder,
    int SingleDoubleSeal,
    string InvoicingInformation,
    int PaymentStatus,
    string WarrantyPeriod,
    bool IsInstallmentPayment,
    decimal AccumulatedAmount);

/// <summary>
/// 创建合同（当前用户为创建人）
/// </summary>
public class CreateContractEndpoint(IMediator mediator) : Endpoint<CreateContractRequest, ResponseData<CreateContractResponse>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractCreate);
        Description(b => b.AutoTagOverride("Contract").WithSummary("创建合同"));
    }

    public override async Task HandleAsync(CreateContractRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateContractCommand(req.Code, req.Title, req.PartyA, req.PartyB,
            req.Amount, req.StartDate, req.EndDate, uid,
            req.FileStorageKey, req.OrderId, req.CustomerId, req.ContractType, req.IncomeExpenseType,
            req.SignDate, req.Note, req.Description,
            req.DepartmentId, req.BusinessManager, req.ResponsibleProject, req.InputCustomer,
            req.NextPaymentReminder, req.ContractExpiryReminder,
            req.SingleDoubleSeal, req.InvoicingInformation, req.PaymentStatus, req.WarrantyPeriod,
            req.IsInstallmentPayment, req.AccumulatedAmount);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateContractResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建合同响应
/// </summary>
/// <param name="Id">合同 ID</param>
public record CreateContractResponse(ContractId Id);
