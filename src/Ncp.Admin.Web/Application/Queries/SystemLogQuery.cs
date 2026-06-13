using Microsoft.Data.Sqlite;
using Ncp.Admin.Web.Services.SystemLogs;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>系统日志列表项。</summary>
public sealed record SystemLogListItemDto(
    long Id,
    DateTimeOffset Timestamp,
    string Level,
    string Category,
    int? EventId,
    string Message,
    bool HasException,
    string? TraceId,
    string? UserId,
    string? RequestPath,
    string? ClientIp);

/// <summary>系统日志详情。</summary>
public sealed record SystemLogDetailDto(
    long Id,
    DateTimeOffset Timestamp,
    string Level,
    string Category,
    int? EventId,
    string Message,
    bool HasException,
    string? TraceId,
    string? UserId,
    string? RequestPath,
    string? ClientIp,
    string? Exception,
    string? PropertiesJson,
    DateTimeOffset CreatedAt);

/// <summary>系统日志筛选选项。</summary>
public sealed record SystemLogOptionsDto(
    IReadOnlyList<string> Levels,
    IReadOnlyList<string> Categories);

/// <summary>系统日志分页查询入参。</summary>
public sealed class SystemLogQueryInput : PageRequest
{
    public string? Level { get; set; }
    public string? Category { get; set; }
    public string? Keyword { get; set; }
    public string? TraceId { get; set; }
    public bool? HasException { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}

/// <summary>系统日志查询（SQLite system_logs 表）。</summary>
public sealed class SystemLogQuery(SystemLogDatabase database) : IQuery
{
    public async Task<PagedData<SystemLogListItemDto>> GetPagedAsync(
        SystemLogQueryInput input,
        CancellationToken cancellationToken = default)
    {
        var pageIndex = input.PageIndex < 1 ? 1 : input.PageIndex;
        var pageSize = input.PageSize < 1 ? 20 : input.PageSize;
        var offset = (pageIndex - 1) * pageSize;

        await using var connection = await database.OpenConnectionAsync(cancellationToken);

        var where = BuildWhereClause(input, out var parameters);
        var total = 0;
        if (input.CountTotal)
        {
            await using var countCommand = connection.CreateCommand();
            countCommand.CommandText = $"SELECT COUNT(*) FROM system_logs WHERE 1=1{where};";
            ApplyParameters(countCommand, parameters);
            total = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken) ?? 0, System.Globalization.CultureInfo.InvariantCulture);
        }

        await using var listCommand = connection.CreateCommand();
        listCommand.CommandText = $"""
            SELECT id, timestamp, level, category, event_id, message, exception, trace_id, user_id, request_path, client_ip
            FROM system_logs
            WHERE 1=1{where}
            ORDER BY timestamp DESC, id DESC
            LIMIT $pageSize OFFSET $offset;
            """;
        ApplyParameters(listCommand, parameters);
        listCommand.Parameters.AddWithValue("$pageSize", pageSize);
        listCommand.Parameters.AddWithValue("$offset", offset);

        var items = new List<SystemLogListItemDto>();
        await using (var reader = await listCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(ReadListItem(reader));
            }
        }

        if (!input.CountTotal)
            total = items.Count;

        return new PagedData<SystemLogListItemDto>(items, total, pageIndex, pageSize);
    }

    public async Task<SystemLogDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, timestamp, level, category, event_id, message, exception, properties_json, trace_id, user_id, request_path, client_ip, created_at
            FROM system_logs
            WHERE id = $id
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var exception = reader.IsDBNull(6) ? null : reader.GetString(6);
        return new SystemLogDetailDto(
            reader.GetInt64(0),
            ParseTimestamp(reader.GetString(1)),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetInt32(4),
            reader.GetString(5),
            !string.IsNullOrWhiteSpace(exception),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.IsDBNull(9) ? null : reader.GetString(9),
            reader.IsDBNull(10) ? null : reader.GetString(10),
            reader.IsDBNull(11) ? null : reader.GetString(11),
            exception,
            reader.IsDBNull(7) ? null : reader.GetString(7),
            ParseTimestamp(reader.GetString(12)));
    }

    public async Task<SystemLogOptionsDto> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);

        var levels = new List<string>();
        await using (var levelCommand = connection.CreateCommand())
        {
            levelCommand.CommandText = """
                SELECT DISTINCT level
                FROM system_logs
                WHERE level IS NOT NULL AND TRIM(level) <> ''
                ORDER BY level;
                """;
            await using var reader = await levelCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                levels.Add(reader.GetString(0));
        }

        var categories = new List<string>();
        await using (var categoryCommand = connection.CreateCommand())
        {
            categoryCommand.CommandText = """
                SELECT DISTINCT category
                FROM system_logs
                WHERE category IS NOT NULL AND TRIM(category) <> ''
                ORDER BY category;
                """;
            await using var reader = await categoryCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                categories.Add(reader.GetString(0));
        }

        return new SystemLogOptionsDto(levels, categories);
    }

    private static string BuildWhereClause(SystemLogQueryInput input, out List<SqliteParameter> parameters)
    {
        parameters = [];
        var where = string.Empty;

        if (!string.IsNullOrWhiteSpace(input.Level))
        {
            where += " AND level = $level";
            parameters.Add(new SqliteParameter("$level", input.Level.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            where += " AND category = $category";
            parameters.Add(new SqliteParameter("$category", input.Category.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(input.TraceId))
        {
            where += " AND trace_id = $traceId";
            parameters.Add(new SqliteParameter("$traceId", input.TraceId.Trim()));
        }

        if (input.HasException == true)
            where += " AND exception IS NOT NULL AND TRIM(exception) <> ''";
        else if (input.HasException == false)
            where += " AND (exception IS NULL OR TRIM(exception) = '')";

        if (input.StartTime.HasValue)
        {
            where += " AND timestamp >= $startTime";
            parameters.Add(new SqliteParameter("$startTime", input.StartTime.Value.ToUniversalTime().ToString("O")));
        }

        if (input.EndTime.HasValue)
        {
            where += " AND timestamp <= $endTime";
            parameters.Add(new SqliteParameter("$endTime", input.EndTime.Value.ToUniversalTime().ToString("O")));
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            where += " AND (message LIKE $keyword OR category LIKE $keyword OR request_path LIKE $keyword OR trace_id LIKE $keyword OR user_id LIKE $keyword)";
            parameters.Add(new SqliteParameter("$keyword", $"%{input.Keyword.Trim()}%"));
        }

        return where;
    }

    private static void ApplyParameters(SqliteCommand command, IReadOnlyList<SqliteParameter> parameters)
    {
        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);
    }

    private static SystemLogListItemDto ReadListItem(SqliteDataReader reader)
    {
        var exception = reader.IsDBNull(6) ? null : reader.GetString(6);
        return new SystemLogListItemDto(
            reader.GetInt64(0),
            ParseTimestamp(reader.GetString(1)),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetInt32(4),
            reader.GetString(5),
            !string.IsNullOrWhiteSpace(exception),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.IsDBNull(9) ? null : reader.GetString(9),
            reader.IsDBNull(10) ? null : reader.GetString(10));
    }

    private static DateTimeOffset ParseTimestamp(string value)
    {
        if (DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            return parsed;
        return DateTimeOffset.MinValue;
    }
}
