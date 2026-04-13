using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Web.Application.Commands.Suppliers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Suppliers;

/// <summary>
/// 删除供应商
/// </summary>
public class DeleteSupplierEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Supplier");
        Description(b => b.AutoTagOverride("Supplier").WithSummary("删除供应商"));
        Delete("/api/admin/suppliers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new SupplierId(Route<Guid>("id"));
        await mediator.Send(new DeleteSupplierCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
