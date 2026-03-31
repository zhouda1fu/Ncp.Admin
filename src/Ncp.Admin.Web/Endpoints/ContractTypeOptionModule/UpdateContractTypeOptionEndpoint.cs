using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptions;
using Ncp.Admin.Web.Application.Commands.ContractTypeOptionModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContractTypeOptionModule;

public record UpdateContractTypeOptionRequest(string Name, int TypeValue, bool OrderSigningCompanyOptionDisplay, int SortOrder);

public class UpdateContractTypeOptionEndpoint(IMediator mediator)
    : Endpoint<UpdateContractTypeOptionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ContractTypeOption");
        Put("/api/admin/contract-type-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractTypeEdit);
    }

    public override async Task HandleAsync(UpdateContractTypeOptionRequest req, CancellationToken ct)
    {
        var id = new ContractTypeOptionId(Route<Guid>("id"));
        await mediator.Send(new UpdateContractTypeOptionCommand(
            id, req.Name, req.TypeValue, req.OrderSigningCompanyOptionDisplay, req.SortOrder), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
