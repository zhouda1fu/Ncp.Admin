using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 获取合同列表请求
/// </summary>
public class GetContractListRequest : ContractQueryInput { }

/// <summary>
/// 获取合同分页列表
/// </summary>
public class GetContractListEndpoint(ContractQuery query)
    : Endpoint<GetContractListRequest, ResponseData<PagedData<ContractQueryDto>>>
{
    public override void Configure()
    {
        Tags("Contract");
        Get("/api/admin/contracts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractView);
    }

    public override async Task HandleAsync(GetContractListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
