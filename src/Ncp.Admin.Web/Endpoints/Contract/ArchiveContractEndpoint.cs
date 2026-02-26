using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 归档合同请求
/// </summary>
public class ArchiveContractRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// 归档合同
/// </summary>
public class ArchiveContractEndpoint(IMediator mediator) : Endpoint<ArchiveContractRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts/{id}/archive");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractArchive);
    }

    public override async Task HandleAsync(ArchiveContractRequest req, CancellationToken ct)
    {
        var cmd = new ArchiveContractCommand(new ContractId(req.Id));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
