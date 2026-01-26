using FastEndpoints;
using FastEndpoints.Swagger;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.DemoEndpoints;

/// <summary>
/// 分布式锁示例
/// </summary>
/// <param name="distributedLock"></param>
public class LockEndpoint(IDistributedLock distributedLock) : EndpointWithoutRequest<ResponseData<bool>>
{
    private static bool _isRunning = false;

    public override void Configure()
    {
        Tags("Demo");
        Description(b => b.AutoTagOverride("Demo"));
        Get("/demo/lock");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_isRunning)
        {
            await Send.OkAsync(true.AsResponseData(), cancellation: ct);
            return;
        }

        await using var handle = await distributedLock.AcquireAsync("lock");
        if (_isRunning)
        {
            await Send.OkAsync(true.AsResponseData(), cancellation: ct);
            return;
        }

#pragma warning disable S2696
        _isRunning = true;
#pragma warning restore S2696
        await Task.Delay(1000, ct);
#pragma warning disable S2696
        _isRunning = false;
#pragma warning restore S2696 
        await Send.OkAsync(false.AsResponseData(), cancellation: ct);
    }
}
