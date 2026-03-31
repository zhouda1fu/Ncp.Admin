using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Web.Middleware;
using Serilog;

namespace Ncp.Admin.Web.Services;

/// <summary>
/// 从 Channel 消费操作日志条目，批量写入数据库。
/// </summary>
public sealed class OperationLogBackgroundService(
    OperationLogChannel channel,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<OperationLogEntry>(BatchSize);
        var lastFlush = DateTimeOffset.UtcNow;

        await foreach (var entry in channel.Reader.ReadAllAsync(stoppingToken))
        {
            buffer.Add(entry);
            var shouldFlush = buffer.Count >= BatchSize ||
                              (buffer.Count > 0 && DateTimeOffset.UtcNow - lastFlush >= FlushInterval);
            if (shouldFlush)
            {
                await FlushAsync(buffer, stoppingToken);
                buffer.Clear();
                lastFlush = DateTimeOffset.UtcNow;
            }
        }

        if (buffer.Count > 0)
            await FlushAsync(buffer, stoppingToken);
    }

    private async Task FlushAsync(List<OperationLogEntry> entries, CancellationToken ct)
    {
        if (entries.Count == 0)
            return;
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entities = entries.Select(e => new OperationLog(
                e.UserId,
                e.UserName,
                e.Module,
                (OperationLogType)e.OperationType,
                e.RequestPath,
                e.RequestMethod,
                e.HttpStatusCode,
                e.IsSuccess,
                e.IpAddress ?? string.Empty,
                e.UserAgent ?? string.Empty,
                e.RequestBody ?? string.Empty,
                e.ResponseBody ?? string.Empty,
                e.DurationMs,
                e.CreatedAt));
            await dbContext.OperationLogs.AddRangeAsync(entities, ct);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "操作日志批量写入失败，条数: {Count}", entries.Count);
        }
    }
}
