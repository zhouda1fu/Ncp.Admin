using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 提交合同审批请求
/// </summary>
public class SubmitContractRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// 提交合同审批
/// </summary>
public class SubmitContractEndpoint(IMediator mediator) : Endpoint<SubmitContractRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts/{id}/submit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractSubmit);
    }

    public override async Task HandleAsync(SubmitContractRequest req, CancellationToken ct)
    {
        var cmd = new SubmitContractCommand(new ContractId(req.Id));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
