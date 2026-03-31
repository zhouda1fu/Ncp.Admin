using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Web.Application.Commands.SupplierModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Supplier;

/// <summary>
/// 更新供应商请求
/// </summary>
public record UpdateSupplierRequest(
    string FullName,
    string ShortName,
    string Contact,
    string Phone,
    string Email = "",
    string Address = "",
    string Remark = "");

/// <summary>
/// 更新供应商
/// </summary>
public class UpdateSupplierEndpoint(IMediator mediator)
    : Endpoint<UpdateSupplierRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Supplier");
        Put("/api/admin/suppliers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(UpdateSupplierRequest req, CancellationToken ct)
    {
        var id = new SupplierId(Route<Guid>("id"));
        var cmd = new UpdateSupplierCommand(
            id,
            req.FullName,
            req.ShortName,
            req.Contact,
            req.Phone,
            req.Email ?? string.Empty,
            req.Address ?? string.Empty,
            req.Remark ?? string.Empty);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
