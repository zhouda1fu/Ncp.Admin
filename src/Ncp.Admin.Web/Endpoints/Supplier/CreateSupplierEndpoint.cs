using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Web.Application.Commands.SupplierModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Supplier;

/// <summary>
/// 创建供应商请求
/// </summary>
public record CreateSupplierRequest(
    string FullName,
    string ShortName,
    string Contact,
    string Phone,
    string Email = "",
    string Address = "",
    string Remark = "");

/// <summary>
/// 创建供应商响应
/// </summary>
public record CreateSupplierResponse(SupplierId Id);

/// <summary>
/// 创建供应商
/// </summary>
public class CreateSupplierEndpoint(IMediator mediator)
    : Endpoint<CreateSupplierRequest, ResponseData<CreateSupplierResponse>>
{
    public override void Configure()
    {
        Tags("Supplier");
        Post("/api/admin/suppliers");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(CreateSupplierRequest req, CancellationToken ct)
    {
        var cmd = new CreateSupplierCommand(
            req.FullName,
            req.ShortName,
            req.Contact,
            req.Phone,
            req.Email ?? string.Empty,
            req.Address ?? string.Empty,
            req.Remark ?? string.Empty);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateSupplierResponse(id).AsResponseData(), cancellation: ct);
    }
}
