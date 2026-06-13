using Microsoft.Extensions.Options;

namespace Ncp.Admin.Web.Services.SystemLogs;

public sealed class SystemLogBackgroundService(
    SystemLogChannel channel,
    SystemLogDatabase database,
    IOptionsMonitor<SystemLogOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<SystemLogEntry>(Math.Max(1, options.CurrentValue.BatchSize));
        var lastFlush = DateTimeOffset.UtcNow;
        var lastCleanup = DateTimeOffset.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var flushInterval = TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.FlushIntervalSeconds));
                while (channel.Reader.TryRead(out var entry))
                    buffer.Add(entry);

                if (buffer.Count >= Math.Max(1, options.CurrentValue.BatchSize)
                    || (buffer.Count > 0 && DateTimeOffset.UtcNow - lastFlush >= flushInterval))
                {
                    await database.InsertBatchAsync(buffer, stoppingToken);
                    buffer.Clear();
                    lastFlush = DateTimeOffset.UtcNow;
                }

                if (DateTimeOffset.UtcNow - lastCleanup >= TimeSpan.FromHours(1))
                {
                    await database.CleanupAsync(stoppingToken);
                    lastCleanup = DateTimeOffset.UtcNow;
                }

                var delayTask = Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                var readTask = channel.Reader.WaitToReadAsync(stoppingToken).AsTask();
                await Task.WhenAny(delayTask, readTask);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        while (channel.Reader.TryRead(out var entry))
            buffer.Add(entry);

        if (buffer.Count > 0)
            await database.InsertBatchAsync(buffer, CancellationToken.None);
    }
}
