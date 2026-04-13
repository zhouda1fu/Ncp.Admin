using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 更新公海客户咨询内容请求
/// </summary>
/// <param name="Id">客户 ID</param>
/// <param name="ConsultationContent">咨询内容</param>
public record UpdateSeaCustomerConsultationRequest(CustomerId Id, string ConsultationContent);

/// <summary>
/// 更新公海客户咨询内容（独立权限）
/// </summary>
public class UpdateSeaCustomerConsultationEndpoint(IMediator mediator)
    : Endpoint<UpdateSeaCustomerConsultationRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customers/{id}/sea/consultation");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaConsultationEdit);
        Description(b => b.AutoTagOverride("Customer").WithSummary("更新公海客户咨询内容"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateSeaCustomerConsultationRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateSeaCustomerConsultationCommand(req.Id, req.ConsultationContent), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
