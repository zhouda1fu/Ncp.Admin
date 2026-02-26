using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 合同详情请求（路由 {id}）
/// </summary>
public record GetContractRequest(ContractId Id);

/// <summary>
/// 按 ID 获取合同详情
/// </summary>
public class GetContractEndpoint(ContractQuery query) : Endpoint<GetContractRequest, ResponseData<ContractQueryDto>>
{
    public override void Configure()
    {
        Tags("Contract");
        Get("/api/admin/contracts/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractView);
    }

    public override async Task HandleAsync(GetContractRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(req.Id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
