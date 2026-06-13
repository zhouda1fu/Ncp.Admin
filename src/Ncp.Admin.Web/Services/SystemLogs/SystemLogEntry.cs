namespace Ncp.Admin.Web.Services.SystemLogs;

public sealed record SystemLogEntry(
    DateTimeOffset Timestamp,
    string Level,
    string Category,
    int? EventId,
    string Message,
    string? Exception,
    string? PropertiesJson,
    string? TraceId,
    string? UserId,
    string? RequestPath,
    string? ClientIp);
