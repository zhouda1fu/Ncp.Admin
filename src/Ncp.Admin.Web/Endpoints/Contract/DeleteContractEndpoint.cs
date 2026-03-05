using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 删除合同（仅草稿可删，软删）
/// </summary>
public class DeleteContractEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Delete("/api/admin/contracts/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractDelete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new ContractId(Route<Guid>("id"));
        await mediator.Send(new DeleteContractCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
