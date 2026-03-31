using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContractTypeOptionModule;

/// <summary>
/// 获取合同类型选项列表
/// </summary>
public class GetContractTypeOptionListEndpoint(ContractTypeOptionQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<ContractTypeOptionDto>>>
{
    public override void Configure()
    {
        Tags("ContractTypeOption");
        Get("/api/admin/contract-type-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractTypeView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
