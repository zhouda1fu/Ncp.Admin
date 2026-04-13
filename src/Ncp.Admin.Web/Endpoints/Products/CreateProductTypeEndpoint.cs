using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Web.Application.Commands.ProductTypes;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 创建产品类型请求
/// </summary>
public record CreateProductTypeRequest(string Name, int SortOrder = 0, bool Visible = true);

/// <summary>
/// 创建产品类型响应
/// </summary>
public record CreateProductTypeResponse(ProductTypeId Id);

/// <summary>
/// 创建产品类型
/// </summary>
public class CreateProductTypeEndpoint(IMediator mediator)
    : Endpoint<CreateProductTypeRequest, ResponseData<CreateProductTypeResponse>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("创建产品类型"));
        Post("/api/admin/product-types");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(CreateProductTypeRequest req, CancellationToken ct)
    {
        var cmd = new CreateProductTypeCommand(req.Name, req.SortOrder, req.Visible);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProductTypeResponse(id).AsResponseData(), cancellation: ct);
    }
}
