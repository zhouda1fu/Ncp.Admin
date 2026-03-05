using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.ContractTypeOption;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContractTypeOption;

public class DeleteContractTypeOptionEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ContractTypeOption");
        Delete("/api/admin/contract-type-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractTypeDelete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await mediator.Send(new DeleteContractTypeOptionCommand(new ContractTypeOptionId(id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
