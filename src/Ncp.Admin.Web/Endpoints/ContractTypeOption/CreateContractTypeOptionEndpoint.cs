using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.ContractTypeOption;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContractTypeOption;

public record CreateContractTypeOptionRequest(string Name, int TypeValue, bool OrderSigningCompanyOptionDisplay = false, int SortOrder = 0);

public record CreateContractTypeOptionResponse(ContractTypeOptionId Id);

public class CreateContractTypeOptionEndpoint(IMediator mediator)
    : Endpoint<CreateContractTypeOptionRequest, ResponseData<CreateContractTypeOptionResponse>>
{
    public override void Configure()
    {
        Tags("ContractTypeOption");
        Post("/api/admin/contract-type-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractTypeCreate);
    }

    public override async Task HandleAsync(CreateContractTypeOptionRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateContractTypeOptionCommand(
            req.Name, req.TypeValue, req.OrderSigningCompanyOptionDisplay, req.SortOrder), ct);
        await Send.OkAsync(new CreateContractTypeOptionResponse(id).AsResponseData(), cancellation: ct);
    }
}
