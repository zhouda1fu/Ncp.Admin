using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Web.Application.Commands.ProductCategoryModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 创建产品分类请求
/// </summary>
public record CreateProductCategoryRequest(
    string Name,
    string Remark,
    Guid? ParentId,
    int SortOrder = 0,
    bool Visible = true,
    bool IsDiscount = false);

/// <summary>
/// 创建产品分类响应
/// </summary>
public record CreateProductCategoryResponse(ProductCategoryId Id);

/// <summary>
/// 创建产品分类
/// </summary>
public class CreateProductCategoryEndpoint(IMediator mediator)
    : Endpoint<CreateProductCategoryRequest, ResponseData<CreateProductCategoryResponse>>
{
    public override void Configure()
    {
        Tags("Product");
        Post("/api/admin/product-categories");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(CreateProductCategoryRequest req, CancellationToken ct)
    {
        var parentId = new ProductCategoryId(Guid.Empty);
        if (req.ParentId != null)
        {
            parentId = new ProductCategoryId(req.ParentId.Value);
        }
        var cmd = new CreateProductCategoryCommand(req.Name, req.Remark, parentId, req.SortOrder, req.Visible, req.IsDiscount);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProductCategoryResponse(id).AsResponseData(), cancellation: ct);
    }
}
