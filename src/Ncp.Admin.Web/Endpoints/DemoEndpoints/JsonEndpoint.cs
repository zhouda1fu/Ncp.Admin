using FastEndpoints;
using FastEndpoints.Swagger;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Primitives;

namespace Ncp.Admin.Web.Endpoints.DemoEndpoints;

public partial record MyId : IGuidStronglyTypedId;

/// <summary>
/// JSON 请求模型
/// </summary>
/// <param name="Id">ID</param>
/// <param name="Name">名称</param>
/// <param name="Time">时间</param>
public record JsonRequest(MyId Id, string Name, DateTime Time);

/// <summary>
/// JSON 响应模型
/// </summary>
/// <param name="Id">ID</param>
/// <param name="Name">名称</param>
/// <param name="Time">时间</param>
public record JsonResponse(MyId Id, string Name, DateTime Time);

/// <summary>
/// JSON 示例
/// </summary>
public class JsonEndpoint : Endpoint<JsonRequest, ResponseData<JsonResponse>>
{
    public override void Configure()
    {
        Tags("Demo");
        Description(b => b.AutoTagOverride("Demo"));
        Post("/demo/json");
        AllowAnonymous();
    }

    public override Task HandleAsync(JsonRequest req, CancellationToken ct)
    {
        return Send.OkAsync(new JsonResponse(req.Id, req.Name, req.Time).AsResponseData(), cancellation: ct);
    }
}