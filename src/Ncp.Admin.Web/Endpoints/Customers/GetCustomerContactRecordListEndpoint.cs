using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 客户联络记录分页查询请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Keyword">关键字</param>
/// <param name="RecordTypeId">联系类型：1电话 2出差 3微信 4其他</param>
/// <param name="StatusId">状态：0待选择 1有效 2无效</param>
/// <param name="OwnerId">按本条联络负责人过滤</param>
/// <param name="RecordAtFrom">联络时间起</param>
/// <param name="RecordAtTo">联络时间止</param>
/// <param name="NextVisitAtFrom">下次回访起</param>
/// <param name="NextVisitAtTo">下次回访止</param>
public record GetCustomerContactRecordListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    int? RecordTypeId = null,
    int? StatusId = null,
    UserId? OwnerId = null,
    DateTimeOffset? RecordAtFrom = null,
    DateTimeOffset? RecordAtTo = null,
    DateTimeOffset? NextVisitAtFrom = null,
    DateTimeOffset? NextVisitAtTo = null);

/// <summary>
/// 客户联络记录分页查询
/// </summary>
/// <param name="query"></param>
public class GetCustomerContactRecordListEndpoint(CustomerContactRecordQuery query)
    : Endpoint<GetCustomerContactRecordListRequest, ResponseData<PagedData<CustomerContactRecordListItemDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Description(b => b.AutoTagOverride("Customer").WithSummary("客户联络记录分页查询"));
        Get("/api/admin/customer-contact-records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactRecordView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetCustomerContactRecordListRequest req, CancellationToken ct)
    {
        var input = new CustomerContactRecordQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Keyword = req.Keyword,
            RecordTypeId = req.RecordTypeId,
            StatusId = req.StatusId,
            OwnerId = req.OwnerId,
            RecordAtFrom = req.RecordAtFrom,
            RecordAtTo = req.RecordAtTo,
            NextVisitAtFrom = req.NextVisitAtFrom,
            NextVisitAtTo = req.NextVisitAtTo,
        };
        var data = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(data.AsResponseData(), cancellation: ct);
    }
}

