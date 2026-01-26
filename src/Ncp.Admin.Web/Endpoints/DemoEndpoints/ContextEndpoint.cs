using FastEndpoints;
using FastEndpoints.Swagger;
using NetCorePal.Context;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.DemoEndpoints;

/// <summary>
/// 上下文示例
/// </summary>
/// <param name="contextAccessor"></param>
public class ContextEndpoint(IContextAccessor contextAccessor) : EndpointWithoutRequest<ResponseData<string>>
{
    public override void Configure()
    {
        Tags("Demo");
        Description(b => b.AutoTagOverride("Demo"));
        Post("/demo/context");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var tenantContext = contextAccessor.GetContext<TenantContext>();
        var result = tenantContext == null ? "" : tenantContext.TenantId;
        return Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}