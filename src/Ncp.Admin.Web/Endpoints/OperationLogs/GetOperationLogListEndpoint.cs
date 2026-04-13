using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.OperationLogs;

/// <summary>
/// 操作日志列表请求（分页与筛选）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="OperatorUserId">操作人用户 ID</param>
/// <param name="Module">模块名称</param>
/// <param name="OperationType">操作类型</param>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
public record GetOperationLogListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    long? OperatorUserId = null,
    string? Module = null,
    OperationLogType? OperationType = null,
    DateTimeOffset? StartTime = null,
    DateTimeOffset? EndTime = null);

/// <summary>
/// 获取操作日志分页列表
/// </summary>
/// <param name="query">操作日志查询服务</param>
public class GetOperationLogListEndpoint(OperationLogQuery query)
    : Endpoint<GetOperationLogListRequest, ResponseData<PagedData<OperationLogListItemDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("OperationLog");
        Description(b => b.AutoTagOverride("OperationLog").WithSummary("获取操作日志分页列表"));
        Get("/api/admin/operation-logs");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OperationLogView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetOperationLogListRequest req, CancellationToken ct)
    {
        var input = new OperationLogQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            OperatorUserId = req.OperatorUserId,
            Module = req.Module,
            OperationType = req.OperationType,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
