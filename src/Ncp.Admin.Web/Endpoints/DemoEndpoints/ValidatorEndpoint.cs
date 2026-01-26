using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Ncp.Admin.Web.Application.Commands.Demos;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.DemoEndpoints;

/// <summary>
/// 校验请求模型
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Price">价格</param>
public record ValidatorRequest(string Name, int Price);

/// <summary>
/// 校验示例
/// </summary>
/// <param name="mediator"></param>
public class ValidatorEndpoint(IMediator mediator) : Endpoint<ValidatorRequest, ResponseData>
{
    public override void Configure()
    {
        Tags("Demo");
        Description(b => b.AutoTagOverride("Demo"));
        Post("/demo/validator");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ValidatorRequest req, CancellationToken ct)
    {
        await mediator.Send(new ValidatorCommand(req.Name, req.Price), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}