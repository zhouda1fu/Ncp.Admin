using System.Diagnostics;
using FastEndpoints;

namespace Ncp.Admin.Web.Processors;

/// <summary>
/// 全局 PreProcessor：为请求创建 Stopwatch，供 PostProcessor 计算耗时。
/// </summary>
public sealed class OperationLogGlobalPreProcessor : IGlobalPreProcessor
{
    public Task PreProcessAsync(IPreProcessorContext ctx, CancellationToken ct)
    {
        // 不在这里做筛选，统一在 PostProcessor 里决定是否记录
        ctx.HttpContext.Items["__oplog_sw"] = Stopwatch.StartNew();
        return Task.CompletedTask;
    }
}

