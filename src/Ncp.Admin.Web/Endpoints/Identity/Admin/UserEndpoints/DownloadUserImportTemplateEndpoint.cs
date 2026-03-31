using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Identity.Admin.UserExcel;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 下载用户导入 Excel 模板（仅表头）。
/// </summary>
public class DownloadUserImportTemplateEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Get("/api/admin/users/excel/import-template");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserImport);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await using var stream = UserExcelWorkbook.CreateTemplateStream();
        await Send.StreamAsync(
            stream,
            "user-import-template.xlsx",
            stream.Length,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            cancellation: ct);
    }
}
