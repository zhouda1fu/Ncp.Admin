using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 获取客户分页列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="FullName">姓名关键字</param>
/// <param name="CustomerSourceId">客户来源</param>
/// <param name="IsVoided">是否作废</param>
/// <param name="OwnerId">负责人</param>
/// <param name="IsInSea">是否在公海</param>
/// <param name="IncludeClaimedSeaCustomers">仅当 IsInSea=true 时生效：是否将“曾领用的公海客户（ClaimedAt!=null）”也纳入公海列表</param>
/// <param name="IsKeyAccount">是否大客户</param>
public record GetCustomerListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? FullName = null,
    CustomerSourceId? CustomerSourceId = null,
    bool? IsVoided = null,
    UserId? OwnerId = null,
    bool? IsInSea = null,
    bool? IncludeClaimedSeaCustomers = null,
    bool? IsKeyAccount = null);

/// <summary>
/// 获取客户分页列表
/// </summary>
/// <param name="query">客户查询</param>
public class GetCustomerListEndpoint(CustomerQuery query)
    : Endpoint<GetCustomerListRequest, ResponseData<PagedData<CustomerQueryDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
        Description(b => b.AutoTagOverride("Customer").WithSummary("获取客户分页列表"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetCustomerListRequest req, CancellationToken ct)
    {
        var input = new CustomerQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            FullName = req.FullName,
            CustomerSourceId = req.CustomerSourceId,
            IsVoided = req.IsVoided,
            OwnerId = req.OwnerId,
            IsInSea = req.IsInSea,
            IncludeClaimedSeaCustomers = req.IncludeClaimedSeaCustomers,
            IsKeyAccount = req.IsKeyAccount,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
