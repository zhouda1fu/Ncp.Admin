using System.Threading.Channels;
using Ncp.Admin.Web.Middleware;

namespace Ncp.Admin.Web.Services;

/// <summary>
/// 操作日志内存队列，中间件写入、后台服务消费。单例注册。
/// </summary>
public sealed class OperationLogChannel
{
    private readonly Channel<OperationLogEntry> _channel = Channel.CreateBounded<OperationLogEntry>(
        new BoundedChannelOptions(10_000) { FullMode = BoundedChannelFullMode.DropWrite });

    /// <summary>
    /// 非阻塞写入一条记录；队列满时丢弃，不阻塞请求。
    /// </summary>
    public bool Write(OperationLogEntry entry) => _channel.Writer.TryWrite(entry);

    /// <summary>
    /// 供后台服务消费的读取端
    /// </summary>
    public ChannelReader<OperationLogEntry> Reader => _channel.Reader;
}
