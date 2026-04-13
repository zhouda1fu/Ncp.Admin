using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.ContractTypeOptions;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContractsTypeOptions;

public record UpdateContractTypeOptionRequest(string Name, int TypeValue, bool OrderSigningCompanyOptionDisplay, int SortOrder);

public class UpdateContractTypeOptionEndpoint(IMediator mediator)
    : Endpoint<UpdateContractTypeOptionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ContractTypeOption");
        Description(b => b.AutoTagOverride("ContractTypeOption").WithSummary("更新合同类型选项"));
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
