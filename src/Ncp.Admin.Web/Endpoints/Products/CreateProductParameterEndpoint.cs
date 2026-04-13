using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Web.Application.Commands.ProductParameters;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 创建产品参数请求
/// </summary>
/// <param name="Year">参数年份（如 "2024"）</param>
/// <param name="Description">参数内容/描述</param>
public record CreateProductParameterRequest(string Year, string Description);

/// <summary>
/// 创建产品参数响应
/// </summary>
/// <param name="Id">参数 ID</param>
public record CreateProductParameterResponse(ProductParameterId Id);

/// <summary>
/// 创建产品参数
/// </summary>
public class CreateProductParameterEndpoint(IMediator mediator)
    : Endpoint<CreateProductParameterRequest, ResponseData<CreateProductParameterResponse>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("创建产品参数"));
        Post("/api/admin/products/{productId}/parameters");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(CreateProductParameterRequest req, CancellationToken ct)
    {
        var productId = new ProductId(Route<Guid>("productId"));
        var cmd = new CreateProductParameterCommand(productId, req.Year, req.Description);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProductParameterResponse(id).AsResponseData(), cancellation: ct);
    }
}
