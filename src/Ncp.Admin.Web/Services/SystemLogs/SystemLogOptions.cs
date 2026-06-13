using Microsoft.Extensions.Logging;

namespace Ncp.Admin.Web.Services.SystemLogs;

public sealed class SystemLogOptions
{
    public const string SectionName = "SystemLog";

    public bool Enabled { get; set; } = true;

    public string DatabasePath { get; set; } = "logs/system-logs.db";

    public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;

    public int RetainDays { get; set; } = 30;

    public int MaxDatabaseSizeMb { get; set; } = 2048;

    public int BatchSize { get; set; } = 100;

    public int FlushIntervalSeconds { get; set; } = 5;

    public int QueueCapacity { get; set; } = 10_000;
}
