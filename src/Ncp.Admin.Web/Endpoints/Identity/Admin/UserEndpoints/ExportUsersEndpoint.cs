using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Identity.Admin.UserExcel;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 按与列表相同的筛选条件导出用户为 Excel（不含密码）。
/// </summary>
/// <param name="userQuery">用户查询</param>
public class ExportUsersEndpoint(UserQuery userQuery) : Endpoint<UserQueryInput>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Get("/api/admin/users/excel/export");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserExport);
    }

    public override async Task HandleAsync(UserQueryInput req, CancellationToken ct)
    {
        var users = await userQuery.GetUsersForExportAsync(req, ct);
        await using var stream = UserExcelWorkbook.CreateExportStream(users);
        var fileName = $"users-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        await Send.StreamAsync(
            stream,
            fileName,
            stream.Length,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            cancellation: ct);
    }
}
