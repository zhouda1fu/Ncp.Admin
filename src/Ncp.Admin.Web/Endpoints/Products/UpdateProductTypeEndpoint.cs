using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Web.Application.Commands.ProductTypes;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 更新产品类型请求
/// </summary>
public record UpdateProductTypeRequest(string Name, int SortOrder, bool Visible);

/// <summary>
/// 更新产品类型
/// </summary>
public class UpdateProductTypeEndpoint(IMediator mediator)
    : Endpoint<UpdateProductTypeRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("更新产品类型"));
        Put("/api/admin/product-types/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(UpdateProductTypeRequest req, CancellationToken ct)
    {
        var id = new ProductTypeId(Route<Guid>("id"));
        var cmd = new UpdateProductTypeCommand(id, req.Name, req.SortOrder, req.Visible);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
