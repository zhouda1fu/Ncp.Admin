using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contracts;

/// <summary>
/// 获取合同列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Code">合同编号</param>
/// <param name="Title">标题</param>
/// <param name="Status">状态</param>
/// <param name="OrderId">订单 ID（Guid 字符串）</param>
/// <param name="CustomerId">客户 ID（Guid 字符串）</param>
/// <param name="ContractType">合同类型</param>
/// <param name="IncomeExpenseType">收支类型</param>
/// <param name="SignDateFrom">签订日起</param>
/// <param name="SignDateTo">签订日止</param>
public record GetContractListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Code = null,
    string? Title = null,
    ContractStatus? Status = null,
    string? OrderId = null,
    string? CustomerId = null,
    int? ContractType = null,
    int? IncomeExpenseType = null,
    DateTimeOffset? SignDateFrom = null,
    DateTimeOffset? SignDateTo = null);

/// <summary>
/// 获取合同分页列表
/// </summary>
public class GetContractListEndpoint(ContractQuery query)
    : Endpoint<GetContractListRequest, ResponseData<PagedData<ContractQueryDto>>>
{
    public override void Configure()
    {
        Tags("Contract");
        Get("/api/admin/contracts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractView);
        Description(b => b.AutoTagOverride("Contract").WithSummary("获取合同分页列表"));
    }

    public override async Task HandleAsync(GetContractListRequest req, CancellationToken ct)
    {
        var input = new ContractQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Code = req.Code,
            Title = req.Title,
            Status = req.Status,
            OrderId = req.OrderId,
            CustomerId = req.CustomerId,
            ContractType = req.ContractType,
            IncomeExpenseType = req.IncomeExpenseType,
            SignDateFrom = req.SignDateFrom,
            SignDateTo = req.SignDateTo,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
