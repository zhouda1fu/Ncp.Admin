namespace Ncp.Admin.Web.Middleware;

/// <summary>
/// 操作日志条目（内存传输 DTO，仅用于中间件 -> Channel -> 后台服务，不入库）
/// </summary>
/// <param name="UserId">操作人用户ID</param>
/// <param name="UserName">操作人姓名</param>
/// <param name="Module">模块名称</param>
/// <param name="OperationType">操作类型（枚举整型）</param>
/// <param name="RequestPath">请求路径</param>
/// <param name="RequestMethod">HTTP 方法</param>
/// <param name="HttpStatusCode">HTTP 状态码</param>
/// <param name="IsSuccess">是否成功</param>
/// <param name="IpAddress">客户端 IP</param>
/// <param name="UserAgent">User-Agent</param>
/// <param name="DurationMs">请求耗时（毫秒）</param>
/// <param name="CreatedAt">操作时间</param>
public record OperationLogEntry(
    long UserId,
    string UserName,
    string Module,
    int OperationType,
    string RequestPath,
    string RequestMethod,
    int HttpStatusCode,
    bool IsSuccess,
    string? IpAddress,
    string? UserAgent,
    string? RequestBody,
    string? ResponseBody,
    long DurationMs,
    DateTimeOffset CreatedAt);
