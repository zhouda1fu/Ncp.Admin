using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 更新合同请求
/// </summary>
public class UpdateContractRequest
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string PartyA { get; set; } = "";
    public string PartyB { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string? FileStorageKey { get; set; }
}

/// <summary>
/// 更新合同
/// </summary>
public class UpdateContractEndpoint(IMediator mediator) : Endpoint<UpdateContractRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Put("/api/admin/contracts/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractEdit);
    }

    public override async Task HandleAsync(UpdateContractRequest req, CancellationToken ct)
    {
        var cmd = new UpdateContractCommand(
            new ContractId(req.Id), req.Code, req.Title, req.PartyA, req.PartyB,
            req.Amount, req.StartDate, req.EndDate, req.FileStorageKey);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
