using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Web.Application.Commands.Product;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 更新产品请求（Id 来自路由）
/// </summary>
/// <param name="Name">产品名称</param>
/// <param name="Code">产品编号</param>
/// <param name="Model">型号</param>
/// <param name="Unit">单位</param>
public record UpdateProductRequest(string Name, string Code, string Model, string Unit);

/// <summary>
/// 更新产品
/// </summary>
public class UpdateProductEndpoint(IMediator mediator)
    : Endpoint<UpdateProductRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Product");
        Put("/api/admin/products/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var id = new ProductId(Route<Guid>("id"));
        var cmd = new UpdateProductCommand(id, req.Name, req.Code, req.Model ?? string.Empty, req.Unit);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
