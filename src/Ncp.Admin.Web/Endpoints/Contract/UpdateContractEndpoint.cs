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
/// <param name="Id">合同 ID</param>
/// <param name="Code">合同编号</param>
/// <param name="Title">标题</param>
/// <param name="PartyA">甲方</param>
/// <param name="PartyB">乙方</param>
/// <param name="Amount">金额</param>
/// <param name="StartDate">开始日期</param>
/// <param name="EndDate">结束日期</param>
/// <param name="FileStorageKey">文件存储键</param>
public record UpdateContractRequest(
    ContractId Id,
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? FileStorageKey);

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
            req.Id, req.Code, req.Title, req.PartyA, req.PartyB,
            req.Amount, req.StartDate, req.EndDate, req.FileStorageKey);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
