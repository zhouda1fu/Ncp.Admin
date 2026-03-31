namespace Ncp.Admin.Web.Application.Excel;

/// <summary>
/// Excel 上传校验通用常量。
/// </summary>
public static class ExcelUploadConstants
{
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    public static readonly string[] AllowedExtensions = [".xlsx", ".xlsm"];

    public static bool IsAllowedExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
}
