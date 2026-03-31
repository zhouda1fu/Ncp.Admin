using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.OperationLog;

/// <summary>
/// 操作日志列表请求（继承分页与筛选）
/// </summary>
public class GetOperationLogListRequest : OperationLogQueryInput { }

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
        Get("/api/admin/operation-logs");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OperationLogView);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetOperationLogListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
