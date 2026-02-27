using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Web.Application.Commands.CustomerSource;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.CustomerSource;

public class UpdateCustomerSourceRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

public class UpdateCustomerSourceEndpoint(IMediator mediator) : Endpoint<UpdateCustomerSourceRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("CustomerSource");
        Put("/api/admin/customer-sources/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSourceEdit);
    }

    public override async Task HandleAsync(UpdateCustomerSourceRequest req, CancellationToken ct)
    {
        var cmd = new UpdateCustomerSourceCommand(new CustomerSourceId(req.Id), req.Name, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
