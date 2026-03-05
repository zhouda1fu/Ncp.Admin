using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 审批通过合同
/// </summary>
public class ApproveContractEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts/{id}/approve");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractApprove);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var id = new ContractId(Route<Guid>("id"));
        var cmd = new ApproveContractCommand(id, new UserId(uid));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
