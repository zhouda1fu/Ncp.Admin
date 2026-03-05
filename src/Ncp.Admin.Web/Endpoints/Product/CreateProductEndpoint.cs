using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Web.Application.Commands.Product;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 创建产品请求
/// </summary>
/// <param name="Name">产品名称</param>
/// <param name="Code">产品编号</param>
/// <param name="Model">型号</param>
/// <param name="Unit">单位</param>
public record CreateProductRequest(string Name, string Code, string Model, string Unit);

/// <summary>
/// 创建产品响应
/// </summary>
/// <param name="Id">产品 ID</param>
public record CreateProductResponse(ProductId Id);

/// <summary>
/// 创建产品
/// </summary>
public class CreateProductEndpoint(IMediator mediator)
    : Endpoint<CreateProductRequest, ResponseData<CreateProductResponse>>
{
    public override void Configure()
    {
        Tags("Product");
        Post("/api/admin/products");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductCreate);
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var cmd = new CreateProductCommand(req.Name, req.Code, req.Model ?? string.Empty, req.Unit);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProductResponse(id).AsResponseData(), cancellation: ct);
    }
}
