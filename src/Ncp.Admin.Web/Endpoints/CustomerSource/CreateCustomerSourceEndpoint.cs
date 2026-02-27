using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Web.Application.Commands.CustomerSource;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.CustomerSource;

public class CreateCustomerSourceRequest
{
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

public record CreateCustomerSourceResponse(CustomerSourceId Id);

public class CreateCustomerSourceEndpoint(IMediator mediator) : Endpoint<CreateCustomerSourceRequest, ResponseData<CreateCustomerSourceResponse>>
{
    public override void Configure()
    {
        Tags("CustomerSource");
        Post("/api/admin/customer-sources");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSourceCreate);
    }

    public override async Task HandleAsync(CreateCustomerSourceRequest req, CancellationToken ct)
    {
        var cmd = new CreateCustomerSourceCommand(req.Name, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateCustomerSourceResponse(id).AsResponseData(), cancellation: ct);
    }
}
