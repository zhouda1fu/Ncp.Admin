using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 审批合同请求
/// </summary>
public class ApproveContractRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// 审批通过合同
/// </summary>
public class ApproveContractEndpoint(IMediator mediator) : Endpoint<ApproveContractRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts/{id}/approve");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractApprove);
    }

    public override async Task HandleAsync(ApproveContractRequest req, CancellationToken ct)
    {
        var cmd = new ApproveContractCommand(new ContractId(req.Id));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
