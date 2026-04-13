using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.IncomeExpenseTypeOptions;

/// <summary>
/// 获取收支类型选项列表
/// </summary>
public class GetIncomeExpenseTypeOptionListEndpoint(IncomeExpenseTypeOptionQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<IncomeExpenseTypeOptionDto>>>
{
    public override void Configure()
    {
        Tags("IncomeExpenseTypeOption");
        Description(b => b.AutoTagOverride("IncomeExpenseTypeOption").WithSummary("获取收支类型选项列表"));
        Get("/api/admin/income-expense-type-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.IncomeExpenseTypeView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
