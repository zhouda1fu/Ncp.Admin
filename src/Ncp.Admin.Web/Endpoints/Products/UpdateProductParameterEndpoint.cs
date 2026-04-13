using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Web.Application.Commands.ProductParameters;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 更新产品参数请求
/// </summary>
/// <param name="Year">参数年份</param>
/// <param name="Description">参数内容/描述</param>
public record UpdateProductParameterRequest(string Year, string Description);

/// <summary>
/// 更新产品参数
/// </summary>
public class UpdateProductParameterEndpoint(IMediator mediator)
    : Endpoint<UpdateProductParameterRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("更新产品参数"));
        Put("/api/admin/product-parameters/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(UpdateProductParameterRequest req, CancellationToken ct)
    {
        var id = new ProductParameterId(Route<Guid>("id"));
        var cmd = new UpdateProductParameterCommand(id, req.Year, req.Description);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
