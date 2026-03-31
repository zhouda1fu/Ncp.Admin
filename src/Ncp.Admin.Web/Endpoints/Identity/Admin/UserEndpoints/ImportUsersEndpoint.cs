using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Application.Excel;
using Ncp.Admin.Web.Application.Identity.Admin.UserExcel;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 用户 Excel 导入请求
/// </summary>
/// <param name="File">Excel 文件（.xlsx）</param>
public record ImportUsersRequest(IFormFile? File);

/// <summary>
/// 从 Excel 批量导入用户（逐行创建，部分失败时返回明细）。
/// </summary>
/// <param name="mediator">MediatR</param>
public class ImportUsersEndpoint(IMediator mediator) : Endpoint<ImportUsersRequest, ResponseData<ImportBatchResultDto>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Post("/api/admin/users/excel/import");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserImport);
        AllowFileUploads();
    }

    public override async Task HandleAsync(ImportUsersRequest req, CancellationToken ct)
    {
        if (req.File == null || req.File.Length == 0)
        {
            throw new KnownException("请上传 Excel 文件", ErrorCodes.InvalidExcelFile);
        }

        if (!ExcelUploadConstants.IsAllowedExtension(req.File.FileName))
        {
            throw new KnownException("仅支持 .xlsx / .xlsm 文件", ErrorCodes.InvalidExcelFile);
        }

        if (req.File.Length > ExcelUploadConstants.MaxUploadBytes)
        {
            throw new KnownException($"文件大小不能超过 {ExcelUploadConstants.MaxUploadBytes / 1024 / 1024} MB", ErrorCodes.InvalidExcelFile);
        }

        await using var uploadStream = req.File.OpenReadStream();
        await using var ms = new MemoryStream();
        await uploadStream.CopyToAsync(ms, ct);
        ms.Position = 0;

        IReadOnlyList<UserImportRowDto> rows;
        try
        {
            rows = UserExcelWorkbook.ParseImportRows(ms);
        }
        catch (KnownException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new KnownException($"无法解析 Excel：{ex.Message}", ErrorCodes.InvalidExcelFile);
        }

        if (rows.Count == 0)
        {
            throw new KnownException("Excel 中没有有效数据行", ErrorCodes.InvalidExcelFile);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var creatorId = !string.IsNullOrWhiteSpace(userIdString) && long.TryParse(userIdString, out var userIdValue)
            ? new UserId(userIdValue)
            : new UserId(0);

        var result = await mediator.Send(new ImportUsersCommand(rows, creatorId), ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
