using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Ncp.Admin.Web.Services.SystemLogs;

public sealed class SystemLogChannel
{
    private readonly Channel<SystemLogEntry> _channel;
    private long _droppedCount;

    public SystemLogChannel(IOptions<SystemLogOptions> options)
    {
        var capacity = Math.Max(100, options.Value.QueueCapacity);
        _channel = Channel.CreateBounded<SystemLogEntry>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false
            });
    }

    public ChannelReader<SystemLogEntry> Reader => _channel.Reader;

    public long DroppedCount => Interlocked.Read(ref _droppedCount);

    public bool Write(SystemLogEntry entry)
    {
        var written = _channel.Writer.TryWrite(entry);
        if (!written)
            Interlocked.Increment(ref _droppedCount);
        return written;
    }
}
