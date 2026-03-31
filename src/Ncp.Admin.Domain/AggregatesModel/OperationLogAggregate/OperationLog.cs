namespace Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;

/// <summary>
/// 操作日志ID（强类型ID，雪花）
/// </summary>
public partial record OperationLogId : IInt64StronglyTypedId;

/// <summary>
/// 操作类型
/// </summary>
public enum OperationLogType
{
    /// <summary>创建</summary>
    Create = 0,
    /// <summary>更新/编辑</summary>
    Update = 1,
    /// <summary>删除</summary>
    Delete = 2,
    /// <summary>提交</summary>
    Submit = 3,
    /// <summary>审批/通过/拒绝</summary>
    Approve = 4,
    /// <summary>其他写操作</summary>
    Other = 5,
}

/// <summary>
/// 操作日志聚合根，用于记录用户对系统的写操作（创建、编辑、删除等），仅追加不修改。
/// </summary>
public class OperationLog : Entity<OperationLogId>, IAggregateRoot
{
    protected OperationLog() { }

    /// <summary>操作人用户ID</summary>
    public long OperatorUserId { get; private set; }
    /// <summary>操作人姓名（冗余，便于展示）</summary>
    public string OperatorUserName { get; private set; } = string.Empty;
    /// <summary>模块名称（如 用户管理、客户管理）</summary>
    public string Module { get; private set; } = string.Empty;
    /// <summary>操作类型</summary>
    public OperationLogType OperationType { get; private set; }
    /// <summary>请求路径</summary>
    public string RequestPath { get; private set; } = string.Empty;
    /// <summary>HTTP 方法</summary>
    public string RequestMethod { get; private set; } = string.Empty;
    /// <summary>HTTP 状态码</summary>
    public int HttpStatusCode { get; private set; }
    /// <summary>是否成功</summary>
    public bool IsSuccess { get; private set; }
    /// <summary>客户端 IP</summary>
    public string IpAddress { get; private set; } = string.Empty;
    /// <summary>User-Agent</summary>
    public string UserAgent { get; private set; } = string.Empty;
    /// <summary>请求入参（JSON，已脱敏/截断）</summary>
    public string RequestBody { get; private set; } = string.Empty;
    /// <summary>响应出参（JSON，已脱敏/截断）</summary>
    public string ResponseBody { get; private set; } = string.Empty;
    /// <summary>请求耗时（毫秒）</summary>
    public long DurationMs { get; private set; }
    /// <summary>操作时间（UTC）</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// 创建一条操作日志记录（仅用于持久化，不发布领域事件）
    /// </summary>
    public OperationLog(
        long operatorUserId,
        string operatorUserName,
        string module,
        OperationLogType operationType,
        string requestPath,
        string requestMethod,
        int httpStatusCode,
        bool isSuccess,
        string ipAddress,
        string userAgent,
        string requestBody,
        string responseBody,
        long durationMs,
        DateTimeOffset createdAt)
    {
        OperatorUserId = operatorUserId;
        OperatorUserName = operatorUserName ?? string.Empty;
        Module = module ?? string.Empty;
        OperationType = operationType;
        RequestPath = requestPath ?? string.Empty;
        RequestMethod = requestMethod ?? string.Empty;
        HttpStatusCode = httpStatusCode;
        IsSuccess = isSuccess;
        IpAddress = ipAddress ?? string.Empty;
        UserAgent = userAgent ?? string.Empty;
        RequestBody = requestBody ?? string.Empty;
        ResponseBody = responseBody ?? string.Empty;
        DurationMs = durationMs;
        CreatedAt = createdAt;
    }
}
