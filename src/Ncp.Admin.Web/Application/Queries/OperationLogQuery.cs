using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 操作日志列表项 DTO
/// </summary>
public record OperationLogListItemDto(
    OperationLogId Id,
    long OperatorUserId,
    string OperatorUserName,
    string Module,
    OperationLogType OperationType,
    string RequestPath,
    string RequestMethod,
    int HttpStatusCode,
    bool IsSuccess,
    string IpAddress,
    string UserAgent,
    string RequestBody,
    string ResponseBody,
    long DurationMs,
    DateTimeOffset CreatedAt);

/// <summary>
/// 操作日志分页查询入参
/// </summary>
public class OperationLogQueryInput : PageRequest
{
    /// <summary>操作人用户ID</summary>
    public long? OperatorUserId { get; set; }
    /// <summary>模块名称</summary>
    public string? Module { get; set; }
    /// <summary>操作类型</summary>
    public OperationLogType? OperationType { get; set; }
    /// <summary>开始时间</summary>
    public DateTimeOffset? StartTime { get; set; }
    /// <summary>结束时间</summary>
    public DateTimeOffset? EndTime { get; set; }
}

/// <summary>
/// 操作日志查询服务
/// </summary>
public class OperationLogQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 分页查询操作日志列表
    /// </summary>
    public async Task<PagedData<OperationLogListItemDto>> GetPagedAsync(OperationLogQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.OperationLogs.AsNoTracking();

        if (input.OperatorUserId.HasValue)
            query = query.Where(x => x.OperatorUserId == input.OperatorUserId.Value);
        if (!string.IsNullOrWhiteSpace(input.Module))
            query = query.Where(x => x.Module == input.Module);
        if (input.OperationType.HasValue)
            query = query.Where(x => x.OperationType == input.OperationType.Value);
        if (input.StartTime.HasValue)
            query = query.Where(x => x.CreatedAt >= input.StartTime.Value);
        if (input.EndTime.HasValue)
            query = query.Where(x => x.CreatedAt <= input.EndTime.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new OperationLogListItemDto(
                x.Id,
                x.OperatorUserId,
                x.OperatorUserName,
                x.Module,
                x.OperationType,
                x.RequestPath,
                x.RequestMethod,
                x.HttpStatusCode,
                x.IsSuccess,
                x.IpAddress,
                x.UserAgent,
                x.RequestBody,
                x.ResponseBody,
                x.DurationMs,
                x.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
