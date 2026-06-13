using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Ncp.Admin.Web.Services.SystemLogs;

public sealed class SystemLogDatabase(
    IOptions<SystemLogOptions> options,
    IWebHostEnvironment environment)
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public string DatabasePath => ResolveDatabasePath(options.Value.DatabasePath);

    public async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var path = DatabasePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var connection = new SqliteConnection($"Data Source={path}");
        await connection.OpenAsync(cancellationToken);
        await ExecuteNonQueryAsync(connection, "PRAGMA busy_timeout = 5000;", cancellationToken);
        await EnsureCreatedAsync(connection, cancellationToken);
        return connection;
    }

    public async Task InsertBatchAsync(IReadOnlyList<SystemLogEntry> entries, CancellationToken cancellationToken)
    {
        if (entries.Count == 0)
            return;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var entry in entries)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (SqliteTransaction)transaction;
            command.CommandText = """
                INSERT INTO system_logs
                (timestamp, level, category, event_id, message, exception, properties_json, trace_id, user_id, request_path, client_ip, created_at)
                VALUES
                ($timestamp, $level, $category, $eventId, $message, $exception, $propertiesJson, $traceId, $userId, $requestPath, $clientIp, $createdAt);
                """;
            command.Parameters.AddWithValue("$timestamp", entry.Timestamp.ToUniversalTime().ToString("O"));
            command.Parameters.AddWithValue("$level", entry.Level);
            command.Parameters.AddWithValue("$category", entry.Category);
            command.Parameters.AddWithValue("$eventId", (object?)entry.EventId ?? DBNull.Value);
            command.Parameters.AddWithValue("$message", entry.Message);
            command.Parameters.AddWithValue("$exception", (object?)entry.Exception ?? DBNull.Value);
            command.Parameters.AddWithValue("$propertiesJson", (object?)entry.PropertiesJson ?? DBNull.Value);
            command.Parameters.AddWithValue("$traceId", (object?)entry.TraceId ?? DBNull.Value);
            command.Parameters.AddWithValue("$userId", (object?)entry.UserId ?? DBNull.Value);
            command.Parameters.AddWithValue("$requestPath", (object?)entry.RequestPath ?? DBNull.Value);
            command.Parameters.AddWithValue("$clientIp", (object?)entry.ClientIp ?? DBNull.Value);
            command.Parameters.AddWithValue("$createdAt", DateTimeOffset.UtcNow.ToString("O"));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var retainDays = Math.Max(1, options.Value.RetainDays);
        var cutoff = DateTimeOffset.UtcNow.AddDays(-retainDays).ToString("O");

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "DELETE FROM system_logs WHERE timestamp < $cutoff;";
            command.Parameters.AddWithValue("$cutoff", cutoff);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await TrimBySizeAsync(connection, cancellationToken);
        await ExecuteNonQueryAsync(connection, "VACUUM;", cancellationToken);
    }

    private async Task TrimBySizeAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var maxBytes = Math.Max(64, options.Value.MaxDatabaseSizeMb) * 1024L * 1024L;
        var file = new FileInfo(DatabasePath);
        if (!file.Exists || file.Length <= maxBytes)
            return;

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM system_logs
            WHERE id IN (
                SELECT id FROM system_logs
                ORDER BY timestamp ASC
                LIMIT (SELECT MAX((SELECT COUNT(*) FROM system_logs) / 10, 1000))
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task EnsureCreatedAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
                return;

            await ExecuteNonQueryAsync(connection, "PRAGMA journal_mode = WAL;", cancellationToken);
            await ExecuteNonQueryAsync(connection, """
                CREATE TABLE IF NOT EXISTS system_logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp TEXT NOT NULL,
                    level TEXT NOT NULL,
                    category TEXT NOT NULL,
                    event_id INTEGER NULL,
                    message TEXT NOT NULL,
                    exception TEXT NULL,
                    properties_json TEXT NULL,
                    trace_id TEXT NULL,
                    user_id TEXT NULL,
                    request_path TEXT NULL,
                    client_ip TEXT NULL,
                    created_at TEXT NOT NULL
                );
                """, cancellationToken);
            await ExecuteNonQueryAsync(connection, "CREATE INDEX IF NOT EXISTS idx_system_logs_timestamp ON system_logs(timestamp DESC);", cancellationToken);
            await ExecuteNonQueryAsync(connection, "CREATE INDEX IF NOT EXISTS idx_system_logs_level_timestamp ON system_logs(level, timestamp DESC);", cancellationToken);
            await ExecuteNonQueryAsync(connection, "CREATE INDEX IF NOT EXISTS idx_system_logs_category_timestamp ON system_logs(category, timestamp DESC);", cancellationToken);
            await ExecuteNonQueryAsync(connection, "CREATE INDEX IF NOT EXISTS idx_system_logs_trace_id ON system_logs(trace_id);", cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task ExecuteNonQueryAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private string ResolveDatabasePath(string path)
    {
        var configured = string.IsNullOrWhiteSpace(path) ? "logs/system-logs.db" : path;
        return Path.GetFullPath(Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured));
    }
}
