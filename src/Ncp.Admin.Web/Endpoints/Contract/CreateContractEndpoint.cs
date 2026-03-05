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
/// <param name="Code">合同编号</param>
/// <param name="Title">标题</param>
/// <param name="PartyA">甲方</param>
/// <param name="PartyB">乙方</param>
/// <param name="Amount">金额</param>
/// <param name="StartDate">开始日期</param>
/// <param name="EndDate">结束日期</param>
/// <param name="FileStorageKey">文件存储键</param>
public record CreateContractRequest(
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? FileStorageKey);

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
