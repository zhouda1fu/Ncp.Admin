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
/// 创建合同请求
/// </summary>
public class CreateContractRequest
{
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
/// 创建合同（当前用户为创建人）
/// </summary>
public class CreateContractEndpoint(IMediator mediator) : Endpoint<CreateContractRequest, ResponseData<CreateContractResponse>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractCreate);
    }

    public override async Task HandleAsync(CreateContractRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateContractCommand(req.Code, req.Title, req.PartyA, req.PartyB,
            req.Amount, req.StartDate, req.EndDate, new UserId(uid), req.FileStorageKey);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateContractResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建合同响应
/// </summary>
public record CreateContractResponse(ContractId Id);
