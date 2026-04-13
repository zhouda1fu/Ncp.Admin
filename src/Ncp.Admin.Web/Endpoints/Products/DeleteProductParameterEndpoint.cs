using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Web.Application.Commands.ProductParameters;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 删除产品参数
/// </summary>
public class DeleteProductParameterEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("删除产品参数"));
        Delete("/api/admin/product-parameters/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new ProductParameterId(Route<Guid>("id"));
        await mediator.Send(new DeleteProductParameterCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
