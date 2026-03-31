using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Supplier;

/// <summary>
/// 获取供应商详情（用于编辑回填）
/// </summary>
public class GetSupplierEndpoint(SupplierQuery query)
    : EndpointWithoutRequest<ResponseData<SupplierDetailDto>>
{
    public override void Configure()
    {
        Tags("Supplier");
        Get("/api/admin/suppliers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new SupplierId(Route<Guid>("id"));
        var result = await query.GetByIdAsync(id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
